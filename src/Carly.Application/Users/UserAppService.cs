using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.IdentityFramework;
using Abp.Linq.Extensions;
using Abp.Localization;
using Abp.Runtime.Session;
using Abp.UI;
using Carly.Authorization;
using Carly.Authorization.Accounts;
using Carly.Authorization.Roles;
using Carly.Authorization.Users;
using Carly.Roles.Dto;
using Carly.Users.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Carly.Emails;
using System.IO;
using Carly.Packages.Dto;
using Carly.Packages;

namespace Carly.Users
{
    [AbpAuthorize(PermissionNames.Pages_Users)]
    public class UserAppService : AsyncCrudAppService<User, UserDto, long, PagedUserResultRequestDto, CreateUserDto, UserDto>, IUserAppService
    {
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;
        private readonly IRepository<Role> _roleRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IAbpSession _abpSession;
        private readonly LogInManager _logInManager;
        private readonly IRepository<Package> _PackageRepository;

        public UserAppService(
            IRepository<User, long> repository,
            UserManager userManager,
            RoleManager roleManager,
            IRepository<Role> roleRepository,
            IPasswordHasher<User> passwordHasher,
            IAbpSession abpSession,
            LogInManager logInManager,
            IRepository<Package> PackageRepository)
            : base(repository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
            _abpSession = abpSession;
            _logInManager = logInManager;
            _PackageRepository = PackageRepository;
        }

        public override async Task<UserDto> CreateAsync(CreateUserDto input)
        {
            CheckCreatePermission();

            var user = ObjectMapper.Map<User>(input);

            user.TenantId = AbpSession.TenantId;
            user.IsEmailConfirmed = true;

            await _userManager.InitializeOptionsAsync(AbpSession.TenantId);

            CheckErrors(await _userManager.CreateAsync(user, input.Password));

            if (input.RoleNames != null)
            {
                CheckErrors(await _userManager.SetRolesAsync(user, input.RoleNames));
            }

            CurrentUnitOfWork.SaveChanges();

            return MapToEntityDto(user);
        }

        public override async Task<UserDto> UpdateAsync(UserDto input)
        {
            CheckUpdatePermission();

            var user = await _userManager.GetUserByIdAsync(input.Id);

            MapToEntity(input, user);

            CheckErrors(await _userManager.UpdateAsync(user));

            if (input.RoleNames != null)
            {
                CheckErrors(await _userManager.SetRolesAsync(user, input.RoleNames));
            }

            return await GetAsync(input);
        }




        public override async Task DeleteAsync(EntityDto<long> input)
        {
            var user = await _userManager.GetUserByIdAsync(input.Id);
            await _userManager.DeleteAsync(user);
        }

        [AbpAuthorize(PermissionNames.Pages_Users_Activation)]
        public async Task Activate(EntityDto<long> user)
        {
            await Repository.UpdateAsync(user.Id, async (entity) =>
            {
                entity.IsActive = true;
            });
        }

        [AbpAuthorize(PermissionNames.Pages_Users_Activation)]
        public async Task DeActivate(EntityDto<long> user)
        {
            await Repository.UpdateAsync(user.Id, async (entity) =>
            {
                entity.IsActive = false;
            });
        }

        public async Task<ListResultDto<RoleDto>> GetRoles()
        {
            var roles = await _roleRepository.GetAllListAsync();
            return new ListResultDto<RoleDto>(ObjectMapper.Map<List<RoleDto>>(roles));
        }

        public async Task ChangeLanguage(ChangeUserLanguageDto input)
        {
            await SettingManager.ChangeSettingForUserAsync(
                AbpSession.ToUserIdentifier(),
                LocalizationSettingNames.DefaultLanguage,
                input.LanguageName
            );
        }

        protected override User MapToEntity(CreateUserDto createInput)
        {
            var user = ObjectMapper.Map<User>(createInput);
            user.SetNormalizedNames();
            return user;
        }

        protected override void MapToEntity(UserDto input, User user)
        {
            ObjectMapper.Map(input, user);
            user.SetNormalizedNames();
        }

        protected override UserDto MapToEntityDto(User user)
        {
            var roleIds = user.Roles.Select(x => x.RoleId).ToArray();

            var roles = _roleManager.Roles.Where(r => roleIds.Contains(r.Id)).Select(r => r.NormalizedName);

            var userDto = base.MapToEntityDto(user);
            userDto.RoleNames = roles.ToArray();

            return userDto;
        }

        protected override IQueryable<User> CreateFilteredQuery(PagedUserResultRequestDto input)
        {
            return Repository.GetAllIncluding(x => x.Roles)
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.UserName.Contains(input.Keyword) || x.Name.Contains(input.Keyword) || x.EmailAddress.Contains(input.Keyword))
                .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive);
        }

        protected override async Task<User> GetEntityByIdAsync(long id)
        {
            var user = await Repository.GetAllIncluding(x => x.Roles).FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
            {
                throw new EntityNotFoundException(typeof(User), id);
            }

            return user;
        }

        protected override IQueryable<User> ApplySorting(IQueryable<User> query, PagedUserResultRequestDto input)
        {
            return query.OrderBy(r => r.UserName);
        }

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }

        public async Task<bool> ChangePassword(ChangePasswordDto input)
        {
            //await _userManager.InitializeOptionsAsync(AbpSession.TenantId);

            var user = await _userManager.FindByIdAsync(AbpSession.GetUserId().ToString());
            if (user == null)
            {
                throw new Exception("There is no current user!");
            }

            if (await _userManager.CheckPasswordAsync(user, input.CurrentPassword))
            {
                CheckErrors(await _userManager.ChangePasswordAsync(user, input.NewPassword));
            }
            else
            {
                CheckErrors(IdentityResult.Failed(new IdentityError
                {
                    Description = "Incorrect password."
                }));
            }

            return true;
        }

        public async Task<bool> ResetPassword(ResetPasswordDto input)
        {
            if (_abpSession.UserId == null)
            {
                throw new UserFriendlyException("Please log in before attempting to reset password.");
            }

            var currentUser = await _userManager.GetUserByIdAsync(_abpSession.GetUserId());
            var loginAsync = await _logInManager.LoginAsync(currentUser.UserName, input.AdminPassword, shouldLockout: false);
            if (loginAsync.Result != AbpLoginResultType.Success)
            {
                throw new UserFriendlyException("Your 'Admin Password' did not match the one on record.  Please try again.");
            }

            if (currentUser.IsDeleted || !currentUser.IsActive)
            {
                return false;
            }

            var roles = await _userManager.GetRolesAsync(currentUser);
            if (!roles.Contains(StaticRoleNames.Tenants.Admin))
            {
                throw new UserFriendlyException("Only administrators may reset passwords.");
            }

            var user = await _userManager.GetUserByIdAsync(input.UserId);
            if (user != null)
            {
                user.Password = _passwordHasher.HashPassword(user, input.NewPassword);
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> SendEmail(EmailContentDto emailContentDto)
        {
            //string FolderName = @"C:\inetpub\wwwroot\ASNBAPI\receipts";
            //if (!System.IO.Directory.Exists(FolderName)) { System.IO.Directory.CreateDirectory(FolderName); }
            //string filepath = FolderName + @"\" + emailContentDto.attachmentFileName;
            //System.IO.File.WriteAllBytes(filepath, emailContentDto.attachment);

            string EmailSubject = emailContentDto.Subject;
            string EmailBody = "<html lang=\"en\">"
                + "<table border=\"0\" cellspacing=\"0\" width=\"100%\" style=\"background:#fff; font-family:quicksand; font-size:18px; line-height:24px\">"
                + "<tbody>"
                + "<tr><td></td>"
                + "<td width=\"600\" style=\"background:#fff; border-radius: 0 0 1rem 1rem\">"
                + "<p style=\"text-align:center; margin: 15px 0\"><img src=\"{LogoImg}\" alt=\"Carly\" width=\"158\" height=\"73\" data-image-whitelisted=\"\" class=\"CToWUd\"></p>"
                //+ "<p style=\"text-align:center; font-family:Quicksand; font-size:45px; line-height:1; margin: 15px 0 25px; color:#fff;\">Affordable Motor Insurance </p>"
                + "</td>"
                + "<td></td></tr>"
                + "<tr><td></td>"
                + "<td>"
                + "<p style=\"text-align:center; font-family:quicksand; font-size:20px; line-height:1; margin: 15px 0 5px; color:#000;\">Thank you for inquiring through us! </p>"
                + "<p style=\"text-align:center; font-family:quicksand; font-size:15px; line-height:1; margin: 15px 0 1px; color:#000;\">Below are your quotation details.</p>"
                + "<tr><td></td>"
                + "<td width=\"600\">"
                + "<p style=\"margin: 20px 0\"></p>"
                + "<div>"
                + "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"max-width:580px; background-color:#f5f5f5; margin-left:10px; border:1px solid #d4d4d4; box-sizing:border-box; padding:10px 20px; border-radius: 0.5rem\">"
                + "<tbody>"
                + "<tr>"
                + "<td valign=\"top\" width=\"290\" style=\"width:290px; font-family:quicksand; font-size:16px; border-bottom:1px solid #d4d4d4; padding:6px 0\">Vehicle Owner</td>"
                + "<td valign=\"top\" width=\"290\" style=\"width:290px; font-family:quicksand; font-size:16px; border-bottom:1px solid #d4d4d4; padding:6px 0\">{VehicleOwnerName}</td>"
                + "</tr>"
                + "<tr>"
                + "<td valign=\"top\" width=\"290\" style=\"width:290px; font-family:quicksand; font-size:16px; border-bottom:1px solid #d4d4d4; padding:6px 0\">Vehicle Registration Number</td>"
                + "<td valign=\"top\" width=\"290\" style=\"width:290px; font-family:quicksand; font-size:16px; border-bottom:1px solid #d4d4d4; padding:6px 0\">{VehicleRegistrationNumber}</td>"
                + "</tr>"
                + "<tr>"
                + "<td valign=\"top\" width=\"290\" style=\"width:290px; font-family:quicksand; font-size:16px; border-bottom:1px solid #d4d4d4; padding:6px 0\">Coverage Period</td>"
                + "<td valign=\"top\" width=\"290\" style=\"width:290px; font-family:quicksand; font-size:16px; border-bottom:1px solid #d4d4d4; padding:6px 0\">{CoveragePeriod}</td>"
                + "</tr>"
                + "<tr>"
                + "<td valign=\"top\" width=\"290\" style=\"width:290px; font-family:quicksand; font-size:16px; padding:6px 0\">Add Ons</td>"
                + "<td valign=\"top\" width=\"290\" style=\"width:290px; font-family:quicksand; font-size:16px; padding:6px 0\">{AddOns}</td>"
                + "</tr>"
                + "</tbody>"
                + "</table>"
                + "</div>"
                + "</td>"
                + "<td></td></tr>"
                + "<tr><td></td>"
                + "<td width=\"600\">"
                + "<p style=\"text-align:center; margin: 10px 0 5px;\"><img src=\"{ImageLink}\" alt=\"Insurance\" style=\"max-width: 40%\" data-image-whitelisted=\"\" class=\"CToWUd\"></p>"
                + "<p style=\"font-family:quicksand; font-size:16px; text-align:center; line-height:1.5; margin:0;\">Lowest Premium</p>"
                + "<p style=\"font-family:quicksand; font-size:30px; text-align:center; line-height:1; margin: 5px 0 5px;\">{Price}</p>"
                + "<p style=\"font-family:quicksand; font-size:16px; text-align:center; line-height:1; margin: 0 0 25px;\"></p>"
                + "</td>"
                + "<td></td></tr>"
                + "<tr><td></td>"
                + "<td width=\"600\" style=\"background:#21bcc1; background:transparent linear-gradient(0deg,#052375 0%,#21bcc1 100%) 0% 0% no-repeat padding-box; padding: 35px 0px; border-radius: 1rem 1rem 0 0\">"
                + "<div>"
                + "<p style=\"text-align:center; margin: 0 0 25px;\"><a href=\"{ViewQuoteLink}\" style=\"background-color:#fff; outline-color:#483d8b; color:#00008B; font-family:quicksand; font-size:16px; padding:18px 35px; text-decoration:none; border-radius:35px; display:inline-block; line-height:1\" "
                + "target=\"_blank\" data-saferedirecturl=\"\"> Compare Quotations </a></p>"
                + "<p style=\"color:#fff; text-align:center; font-size:12px; line-height:1; margin:0 0 8px\"> Having trouble clicking the link? Click link below to check out our website&gt;</p>"
                + "<p style=\"text-align:center; font-size:12px; line-height:1; margin:0 0 8px\"><a href=\"https://www.carly.com.my/\" style=\"color:#fff; text-decoration:underline\" target=\"_blank\">https://www.carly.com.my/</a></p>"
                + "<p style=\"color:#fff; text-align:center; font-size:12px; line-height:1; margin:0 0 8px\"> or Contact us through</p>"
                + "<p style=\"color:#fff; text-align:center; font-size:12px; line-height:1; margin:0 0 8px\"> 017-8656141</p>"
                //+ "<p style=\"text-align:center; color:#fff\"> Pay With </p>"
                + "</div>"
                + "</tbody>"
                + "</table>"
                + "</html>";



            string filepath = "";
            //using streamreader for reading my htmltemplate   


            //SK_KioskModule KioskModuleObj = _kioskModuleRepository.GetAsync(emailContentDto.Module).Result;
            //if (!Equals(KioskModuleObj, null))
            //{
            //    EmailSubject = Equals(emailContentDto.language, "MS") ? KioskModuleObj.SubjectBM : KioskModuleObj.SubjectEN;
            //    EmailBody = Equals(emailContentDto.language, "MS") ? KioskModuleObj.MessageBM : KioskModuleObj.MessageEN;
            //    EmailBody = EmailBody.Replace(@"<Name>", emailContentDto.Name).Replace(@"<IC>", emailContentDto.IC).Replace(@"<UnitHolderID>", emailContentDto.UnitHolderID).Replace(@"<ModuleName>", KioskModuleObj.KM_Name).Replace(@"<TrxDate>", emailContentDto.TrxDate.ToString("dd-MMM-yyyy HH:mm:ss"));
            //}
            EmailBody = EmailBody.Replace(@"{VehicleOwnerName}", emailContentDto.VehicleOwnerName)
                .Replace(@"{VehicleRegistrationNumber}", emailContentDto.VehicleRegistrationNumber)
                .Replace(@"{CoveragePeriod}", emailContentDto.CoveragePeriod)
                .Replace(@"{AddOns}", emailContentDto.AddOns)
                .Replace(@"{ImageLink}", emailContentDto.ImageLink)
                .Replace(@"{Price}", emailContentDto.Price)
                .Replace(@"{ViewQuoteLink}", emailContentDto.ViewQuoteLink)
                .Replace(@"{LogoImg}", "https://system.carly.com.my/CarlyImage/carly-logo.png");

            Emails.IEmailAppService emailAppService = new Emails.EmailAppService(SettingManager);


            bool isEmailSent = await emailAppService.SendEmailAsync(emailContentDto.emailTo, EmailSubject, EmailBody, filepath);

            if (isEmailSent)
            {
                List<Package> packageList = _PackageRepository.GetAll().Where(x => x.VehicleRegNo == emailContentDto.VehicleRegistrationNumber).ToList();

                foreach (var pack in packageList)
                {
                    pack.Status = DateTime.Now.ToString("yyyy-MM-dd");
                    await _PackageRepository.UpdateAsync(pack);
                }
            }

            return isEmailSent;

            //return await emailAppService.SendEmailAsync(emailContentDto.emailTo, EmailSubject, EmailBody, filepath);



        }

        public async Task<bool> SendEmailReminder(EmailContentDto emailContentDto)
        {
            //string FolderName = @"C:\inetpub\wwwroot\ASNBAPI\receipts";
            //if (!System.IO.Directory.Exists(FolderName)) { System.IO.Directory.CreateDirectory(FolderName); }
            //string filepath = FolderName + @"\" + emailContentDto.attachmentFileName;
            //System.IO.File.WriteAllBytes(filepath, emailContentDto.attachment);

            string EmailSubject = emailContentDto.Subject;
            string EmailBody = "<html lang=\"en\">"
                + "<table border=\"0\" cellspacing=\"0\" width=\"100%\" style=\"background:#fff; font-family:quicksand; font-size:18px; line-height:24px\">"
                + "<tbody>"
                + "<tr><td></td>"
                + "<td width=\"600\" style=\"background:#fff; border-radius: 0 0 1rem 1rem\">"
                + "<p style=\"text-align:center; margin: 15px 0\"><img src=\"{LogoImg}\" alt=\"Carly\" width=\"158\" height=\"73\" data-image-whitelisted=\"\" class=\"CToWUd\"></p>"
                //+ "<p style=\"text-align:center; font-family:Quicksand; font-size:45px; line-height:1; margin: 15px 0 25px; color:#fff;\">Affordable Motor Insurance </p>"
                + "</td>"
                + "<td></td></tr>"
                + "<tr><td></td>"
                + "<td>"
                + "<p style=\"text-align:center; font-family:quicksand; font-size:20px; line-height:1; margin: 15px 0 5px; color:#000;\">Hey {VehicleOwnerName}! It has been 14 Days since we last heard from you!</p>"
                + "<p style=\"text-align:center; font-family:quicksand; font-size:18px; line-height:1; margin: 15px 0 1px; color:#000;\">Get the best insurance price with us now!</p>"
                + "<tr><td></td>"
                + "<td width=\"600\">"
                + "<p style=\"margin: 20px 0\"></p>"
                + "<div>"
                + "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"max-width:580px; background-color:#f5f5f5; margin-left:10px; border:1px solid #d4d4d4; box-sizing:border-box; padding:10px 20px; border-radius: 0.5rem\">"
                + "<tbody>"
                + "<tr>"
                + "<td valign=\"top\" width=\"290\" style=\"width:290px; font-family:quicksand; font-size:16px; border-bottom:1px solid #d4d4d4; padding:6px 0\">Vehicle Owner</td>"
                + "<td valign=\"top\" width=\"290\" style=\"width:290px; font-family:quicksand; font-size:16px; border-bottom:1px solid #d4d4d4; padding:6px 0\">{VehicleOwnerName}</td>"
                + "</tr>"
                + "<tr>"
                + "<td valign=\"top\" width=\"290\" style=\"width:290px; font-family:quicksand; font-size:16px; border-bottom:1px solid #d4d4d4; padding:6px 0\">Vehicle Registration Number</td>"
                + "<td valign=\"top\" width=\"290\" style=\"width:290px; font-family:quicksand; font-size:16px; border-bottom:1px solid #d4d4d4; padding:6px 0\">{VehicleRegistrationNumber}</td>"
                + "</tr>"
                + "<tr>"
                + "<td valign=\"top\" width=\"290\" style=\"width:290px; font-family:quicksand; font-size:16px; border-bottom:1px solid #d4d4d4; padding:6px 0\">Coverage Period</td>"
                + "<td valign=\"top\" width=\"290\" style=\"width:290px; font-family:quicksand; font-size:16px; border-bottom:1px solid #d4d4d4; padding:6px 0\">{CoveragePeriod}</td>"
                + "</tr>"
                + "<tr>"
                + "<td valign=\"top\" width=\"290\" style=\"width:290px; font-family:quicksand; font-size:16px; padding:6px 0\">Add Ons</td>"
                + "<td valign=\"top\" width=\"290\" style=\"width:290px; font-family:quicksand; font-size:16px; padding:6px 0\">{AddOns}</td>"
                + "</tr>"
                + "</tbody>"
                + "</table>"
                + "</div>"
                + "</td>"
                + "<td></td></tr>"
                + "<tr><td></td>"
                + "<td width=\"600\">"
                + "<p style=\"text-align:center; margin: 10px 0 5px;\"><img src=\"{ImageLink}\" alt=\"Insurance\" style=\"max-width: 40%\" data-image-whitelisted=\"\" class=\"CToWUd\"></p>"
                + "<p style=\"font-family:quicksand; font-size:16px; text-align:center; line-height:1.5; margin:0;\">Lowest Premium</p>"
                + "<p style=\"font-family:quicksand; font-size:30px; text-align:center; line-height:1; margin: 5px 0 5px;\">{Price}</p>"
                + "<p style=\"font-family:quicksand; font-size:16px; text-align:center; line-height:1; margin: 0 0 25px;\"></p>"
                + "</td>"
                + "<td></td></tr>"
                + "<tr><td></td>"
                + "<td width=\"600\" style=\"background:#21bcc1; background:transparent linear-gradient(0deg,#052375 0%,#21bcc1 100%) 0% 0% no-repeat padding-box; padding: 35px 0px; border-radius: 1rem 1rem 0 0\">"
                + "<div>"
                + "<p style=\"text-align:center; margin: 0 0 25px;\"><a href=\"{ViewQuoteLink}\" style=\"background-color:#fff; outline-color:#483d8b; color:#00008B; font-family:quicksand; font-size:16px; padding:18px 35px; text-decoration:none; border-radius:35px; display:inline-block; line-height:1\" "
                + "target=\"_blank\" data-saferedirecturl=\"\"> Compare Quotations </a></p>"
                + "<p style=\"color:#fff; text-align:center; font-size:12px; line-height:1; margin:0 0 8px\"> Having trouble clicking the link? Click link below to check out our website&gt;</p>"
                + "<p style=\"text-align:center; font-size:12px; line-height:1; margin:0 0 8px\"><a href=\"https://www.carly.com.my/\" style=\"color:#fff; text-decoration:underline\" target=\"_blank\">https://www.carly.com.my/</a></p>"
                + "<p style=\"color:#fff; text-align:center; font-size:12px; line-height:1; margin:0 0 8px\"> or Contact us through</p>"
                + "<p style=\"color:#fff; text-align:center; font-size:12px; line-height:1; margin:0 0 8px\"> 017-8656141</p>"
                //+ "<p style=\"text-align:center; color:#fff\"> Pay With </p>"
                + "</div>"
                + "</tbody>"
                + "</table>"
                + "</html>";



            string filepath = "";
            //using streamreader for reading my htmltemplate   


            //SK_KioskModule KioskModuleObj = _kioskModuleRepository.GetAsync(emailContentDto.Module).Result;
            //if (!Equals(KioskModuleObj, null))
            //{
            //    EmailSubject = Equals(emailContentDto.language, "MS") ? KioskModuleObj.SubjectBM : KioskModuleObj.SubjectEN;
            //    EmailBody = Equals(emailContentDto.language, "MS") ? KioskModuleObj.MessageBM : KioskModuleObj.MessageEN;
            //    EmailBody = EmailBody.Replace(@"<Name>", emailContentDto.Name).Replace(@"<IC>", emailContentDto.IC).Replace(@"<UnitHolderID>", emailContentDto.UnitHolderID).Replace(@"<ModuleName>", KioskModuleObj.KM_Name).Replace(@"<TrxDate>", emailContentDto.TrxDate.ToString("dd-MMM-yyyy HH:mm:ss"));
            //}
            EmailBody = EmailBody.Replace(@"{VehicleOwnerName}", emailContentDto.VehicleOwnerName)
                .Replace(@"{VehicleRegistrationNumber}", emailContentDto.VehicleRegistrationNumber)
                .Replace(@"{CoveragePeriod}", emailContentDto.CoveragePeriod)
                .Replace(@"{AddOns}", emailContentDto.AddOns)
                .Replace(@"{ImageLink}", emailContentDto.ImageLink)
                .Replace(@"{Price}", emailContentDto.Price)
                .Replace(@"{ViewQuoteLink}", emailContentDto.ViewQuoteLink)
                .Replace(@"{LogoImg}", "https://system.carly.com.my/CarlyImage/carly-logo.png");

            Emails.IEmailAppService emailAppService = new Emails.EmailAppService(SettingManager);


            bool isEmailSent = await emailAppService.SendEmailAsync(emailContentDto.emailTo, EmailSubject, EmailBody, filepath);

            return isEmailSent;

            //return await emailAppService.SendEmailAsync(emailContentDto.emailTo, EmailSubject, EmailBody, filepath);



        }

        public async Task<bool> SendSalesReceiptEmail(SalesReceiptContentDto salesReceiptContentDto)
        {
            string EmailSubject = salesReceiptContentDto.Subject;
            string EmailBody = "<html lang =\"en\">"
                + "<table border =\"0\" cellspacing =\"0\" width =\"100%\" "
                + "style =\"background:#fff; font-family:quicksand; font-size:18px; line-height:24px\"> "
                + "<tbody><tr>"
                + "<td bgcolor =\"#ffffff\" >"
                + "<table width =\"600\" align = \"center\" border = \"0\" cellspacing = \"0\" cellpadding = \"0\" bgcolor = \"#ffffff\"> "
                + "<tbody><tr>"
                //+ "<td valign = \"top\" width = \"45\" ></td>"
                + "<td valign = \"top\" style = \"font-family:quicksand; color:#000000; font-size:11px\">"
                + "<table width = \"100%\" border = \"0\" cellspacing = \"0\" cellpadding = \"0\">"
                + "<tbody><tr>"
                + "<td width = \"600\" style = \"background:#fff; border-radius: 0 0 1rem 1rem\">"
                + "<p style = \"text-align:center; margin: 10px 0\">"
                + "<img src = \"{LogoImg}\" alt = \"Carly\" width = \"158\" height = \"73\" data-image-whitelisted = \"\" class =\"CToWUd\"/>"
                + "</p></td>"
                + "<td width = \"600\" style=\"background:#fff; border-radius: 0 0 1rem 1rem\">"
                + "<p style = \"text-align:left; margin: 1px 0\">My Works Sdn Bhd(1340955 - H)</p>"
                + "<p style = \"text-align:left; margin: 1px 0\">No. 1, Solok Sultan Mohamed 1,</p>"
                + "<p style = \"text-align:left; margin: 1px 0\">Pusat Perdagangan Bandar Sultan Saleiman 4,</p>"
                + "<p style = \"text-align:left; margin: 1px 0\">42000 Port Klang, Selangor.</p>"
                + "<p style = \"text-align:left; margin: 1px 0\">T: +6017 - 865 6141</p>"
                + "<p style = \"text-align:left; margin: 1px 0\">E: hello @carly.com.my</p>"
                + "</td></tr>"
                + "<tr><td width = \"600\" style= \"background:#fff; border-radius: 0 0 1rem 1rem\">"
                + "<p style= \"text-align:right; font-family:quicksand; font-size:32px; line-height:20px; font-weight:bold; color:#00008B;\">INVOICE</p>"
                + "</td></tr>"
                + "</tbody></table>"
                + "<tbody><tr><td>"
                + "<table width = \"100%\" border= \"0\" cellspacing= \"0\" cellpadding= \"0\">"
                + "<tbody><tr><td style= \"background:#fff; border-radius: 0 0 1rem 1rem\">"
                + "<p style= \"inline-size: 100px; font-family:quicksand; font-size:15px; line-height:15px; font-weight:bold\">Onwer</p>"
                + "</td><td style= \"background:#fff; border-radius: 0 0 1rem 1rem\">"
                + "<p style= \"inline-size: 400px; font-family:quicksand; font-size:15px; line-height:15px; font-weight:bold; overflow-wrap: break-word;\">{VehicleOwnerName}</p>"
                + "</td></tr>"
                + "<tr><td style = \"background:#fff; border-radius: 0 0 1rem 1rem\">"
                + "<p style= \"inline-size: 100px; font-family:quicksand; font-size:15px; line-height:15px; font-weight:bold\">ID Number</p>"
                + "</td><td style = \"background:#fff; border-radius: 0 0 1rem 1rem\">"
                + "<p style = \"inline-size: 400px; font-family:quicksand; font-size:15px; line-height:15px; font-weight:bold; overflow-wrap: break-word;\">{VehicleICNumber}</p>"
                + "</td></tr>"
                + "<tr><td style= \"background:#fff; border-radius: 0 0 1rem 1rem\">"
                + "<p style= \"inline-size: 100px; font-family:quicksand; font-size:15px; line-height:15px; font-weight:bold\">Address</p>"
                + "</td><td style= \"background:#fff; border-radius: 0 0 1rem 1rem\">"
                + "<p style= \"inline-size: 400px; font-family:quicksand; font-size:15px; line-height:15px; font-weight:bold; overflow-wrap: break-word;\">{VehicleOwnerAddress}</p>"
                + "</td></tr>"
                + "</tbody></table>"
                + "<table width = \"100%\" border = \"0\" cellspacing = \"0\" cellpadding = \"0\">"
                + "<td valign = \"top\" width= \"45\" ></td>"
                + "<tr><td style= \"background:#fff; border-radius: 0 0 1rem 1rem\">"
                + "<p style= \"inline-size: 120px; font-family:quicksand; font-size:15px; line-height:12px; font-weight:bold; overflow-wrap: break-word;\">Vehicle Reg.No.</p>"
                + "</td><td style = \"background:#fff; border-radius: 0 0 1rem 1rem\">"
                + "<p style = \"inline-size: 180px; font-family:quicksand; font-size:15px; line-height:12px; font-weight:bold; overflow-wrap: break-word;\">{VehicleRegistrationNumber}</p>"
                + "</td><td><p style = \"inline-size: 120px; font-family:quicksand; font-size:15px; line-height:10px; font-weight:bold; overflow-wrap: break-word;\">Insurer</p>"
                + "</td><td style = \"background:#fff; border-radius: 0 0 1rem 1rem\">"
                + "<p style = \"inline-size: 180px; font-family:quicksand; font-size:15px; line-height:10px; font-weight:bold; overflow-wrap: break-word;\">{Insurer}</p>"
                + "</td></tr><tr><td style = \"background:#fff; border-radius: 0 0 1rem 1rem\">"
                + "<p style = \"inline-size: 120px; font-family:quicksand; font-size:15px; line-height:10px; font-weight:bold; overflow-wrap: break-word;\">Sum Insured</p>"
                + "</td><td style = \"background:#fff; border-radius: 0 0 1rem 1rem\">"
                + "<p style= \"inline-size: 180px; font-family:quicksand; font-size:15px; line-height:12px; font-weight:bold; overflow-wrap: break-word;\">{SumInsured}</p>"
                + "</td><td><p style = \"inline-size: 120px; font-family:quicksand; font-size:15px; line-height:12px; font-weight:bold; overflow-wrap: break-word;\">Type of Cover</p>"
                + "</td><td style = \"background:#fff; border-radius: 0 0 1rem 1rem\">"
                + "<p style = \"inline-size: 180px; font-family:quicksand; font-size:15px; line-height:12px; font-weight:bold; overflow-wrap: break-word;\">{TypeOfCover}</p>"
                + "</td></tr>"
                + "<tr><td><p style = \"inline-size: 120px; font-family:quicksand; font-size:15px; line-height:12px; font-weight:bold; overflow-wrap: break-word;\">Invoice no</p>"
                + "</td><td style = \"background:#fff; border-radius: 0 0 1rem 1rem\">"
                + "<p style = \"inline-size: 180px; font-family:quicksand; font-size:15px; line-height:12px; font-weight:bold; overflow-wrap: break-word;\">{InvoiceNo}</p>"
                + "</td><td style = \"background:#fff; border-radius: 0 0 1rem 1rem\">"
                + "<p style = \"inline-size: 120px; font-family:quicksand; font-size:15px; line-height:12px; font-weight:bold; overflow-wrap: break-word;\">Period of Cover</p>"
                + "</td><td style = \"background:#fff; border-radius: 0 0 1rem 1rem\">"
                + "<p style = \"inline-size: 180px; font-family:quicksand; font-size:15px; line-height:12px; font-weight:bold; overflow-wrap: break-word;\">{CoveragePeriod}</p>"
                + "</td></tr></table>"
                + "</td></tr></tbody></td><td valign = \"top\" width= \"45\"></td></tr></tbody></table>"
                + "<table width = \"600\" align = \"center\" border = \"0\" cellspacing = \"0\" cellpadding = \"0\" bgcolor = \"#f4f4f4\">"
                + "<tbody><tr><td valign = \"top\" width = \"45\"></td><td align = \"center\" valign = \"top\">"
                + "<table border = \"0\" cellspacing = \"0\" cellpadding = \"0\" width = \"100%\">"
                + "<tbody><tr><td align = \"left\" height = \"20\"></td>"
                + "</tr></tbody></table>"
                + "<table border = \"0\" cellspacing = \"0\" cellpadding = \"0\" width = \"100%\">"
                + "<tbody><tr><td width = \"55%\" align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold; color:#00af41\"></td></tr></tbody></table>"
                + "<table width = \"100%\" border = \"0\" cellspacing = \"0\" cellpadding = \"0\">"
                + "<tbody><tr><td valign = \"top\" width = \"207\" style = \"max-width:207px; display:block\">"
                + "<table width = \"100%\" border = \"0\" cellspacing = \"0\" cellpadding = \"0\">"
                + "<tbody><tr><td align = \"left\" valign = \"top\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">Transaction Details</td>"
                + "</tr><tr><td align = \"center\" valign = \"middle\" height = \"10\"></td></tr>"
                + "<tr><td valign = \"top\">"
                + "<table width = \"100%\" border = \"0\" cellpadding = \"0\" cellspacing = \"0\">"
                + "<tbody><tr><td align = \"left\" valign = \"top\" style = \"padding:0cm 0cm 0cm 0cm\">"
                + "<table width = \"100%\" border = \"0\" cellpadding = \"0\" cellspacing = \"0\">"
                + "<tbody><tr><td valign = \"top\">"
                + "<table width = \"100%\" border = \"0\" cellspacing = \"0\" cellpadding = \"0\">"
                + "<tbody><tr><td align = \"left\" valign = \"top\">"
                + "<table border = \"0\" cellspacing = \"0\" cellpadding = \"0\" width = \"100%\">"
                + "<tbody><tr><td align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:12px; color:#9e9e9e; line-height:16px\">Transaction Date</span><br/>"
                + "<span style = \"font-family:quicksand; font-size:12px;line-height:16px;font-weight:bold\">{TransactionDate}</span>"
                + "</td></tr></tbody></table></td></tr><tr></tr><tr><td align = \"left\" valign = \"top\">"
                + "<table border = \"0\" cellspacing = \"0\" cellpadding=\"0\" width=\"100%\">"
                + "<tbody><tr><td align = \"left\" style = \"font-family:quicksand; font-size:14px;font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:12px; color:#9e9e9e; line-height:14px\">Cardholder's Name</span><br />"
                + "<span style = \"font-family:quicksand; font-size:12px; line-height:16px; font-weight:bold\">{CardHolderName}</span>"
                + "</td></tr></tbody></table></td></tr>"
                + "<tr><td align = \"left\" valign =\"top\"><table border = \"0\" cellspacing =\"0\" cellpadding=\"0\" width=\"100%\">"
                + "<tbody><tr><td align = \"left\" style =\"font-family:quicksand; font-size:14px;font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:12px; color:#9e9e9e; line-height:14px\">Authorization Code</span><br/>"
                + "<span style = \"font-family:quicksand; font-size:12px; line-height:16px; font-weight:bold\">{AuthCode}</span>"
                + "</td></tr></tbody></table></td></tr>"
                + "<tr><td height =\"3\"></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table>"
                + "</td></tr></tbody></table></td><td valign = \"top\" width=\"9\"></td><td valign = \"top\" width=\"10\" bgcolor=\"#f5f5f3\"></td>"
                + "<td valign = \"top\" width=\"280\" style=\"max-width:280px\">"
                + "<table width = \"100%\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">"
                + "<tbody><tr><td align = \"left\" valign=\"top\" style=\"font-family:quicksand; font-size:14px; font-weight:bold;\">Receipt Summary</td></tr>"
                + "<tr><td align = \"center\" valign= \"middle\" height= \"10\" ></td></tr>"
                + "<tr><td valign = \"top\">"
                + "<table width =\"100%\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" bgcolor=\"#ffffff\" style=\"border:1px solid #dddddd\">"
                + "<tbody><tr><td align = \"left\" valign=\"top\" style=\"padding:0cm 0cm 0cm 0cm\">"
                + "<table width = \"100%\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\">"
                + "<tbody><tr><td valign = \"top\" ><table width =\"100%\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">"
                + "<tbody><tr><td align = \"left\" valign=\"top\"><table border = \"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"100%\">"
                + "<tr><td height = \"10px\" align=\"left\"></td><td height = \"10px\" colspan=\"2\" align=\"left\"></td><td height = \"10px\" align=\"left\"></td></tr>"
                + "<tbody><tr><td height = \"5px\" align=\"left\"></td><td height = \"5px\" colspan=\"2\" align=\"left\" style=\"font-family:quicksand; font-size:11px;line-height:18px; color:#9e9e9e\">"
                + "Payment Method:<br /><span style = \"font-weight:bold;color:#000000\"> {PaymentMethod}&nbsp;&nbsp;</span></td>"
                + "<td height = \"5px\" align=\"left\"></td></tr><tr><td height = \"5px\" align =\"left\"></td><td height = \"5px\" colspan =\"2\" align =\"left\"></td>"
                + "<td height = \"5px\" align =\"left\"></td></tr>"
                + "<tr><td height = \"3px\" align =\"left\"></td><td height = \"3px\" colspan =\"2\" align =\"left\" style =\"border-top:1px dashed #9e9e9e\"></td>"
                + "<td height = \"3px\" align =\"left\"></td></tr>"
                + "<tr><td align = \"left\" width =\"15\"></td><td width = \"171\" align =\"left\" style =\"font-family:quicksand; font-weight:normal; color:#000000\">"
                + "<span style = \"font-family:quicksand; font-size:11px; color:#9e9e9e; line-height:21px\"> Description:</span></td>"
                + "<td width = \"80\" align =\"left\" style =\"font-family:quicksand; font-weight:normal; color:#000000\">"
                + "<span style = \"font-family:quicksand; font-size:11px; color:#9e9e9e; line-height:28px\"> &nbsp; &nbsp; &nbsp; &nbsp; Amount:</span></td>"
                + "<td align = \"left\" width =\"15\"></td></tr>"
                + "<tr><td height = \"3px\" align =\"left\"></td>"
                + "<td height = \"3px\" colspan =\"2\" align =\"left\"></td>"
                + "<td height = \"3px\" align =\"left\"></td></tr>"
                + "<tr><td height = \"5px\" align =\"left\"></td><td height = \"5px\" colspan =\"2\" align =\"left\" style =\"border-top:1px dashed #9e9e9e\"></td>"
                + "<td height = \"5px\" align =\"left\"></td></tr>"
                + "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:11px; line-height:18px\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"></span>"
                + "<span style = \"font-family:quicksand; font-size:11px; font-weight:bold\"> Basic Premium</span></td>"
                + "<td align = \"left\" style = \"font-family:quicksand; font-size:11px; line-height:18px\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px; font-weight:bold\"> &nbsp; &nbsp; &nbsp; &nbsp; RM 1,265.91 </span></td>"
                + "<td align = \"right\" width =\"15\"></td></tr>"
                + "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; + Loading 1 </span></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:11px; line-height:18px\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {loading1}</span></td>"
                + "<td align = \"right\" width =\"15\"></td><td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; &nbsp;</span></td>"
                + "<td align = \"right\" width =\"15\"></td></tr>"
                + "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; + Loading 2</span></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {loading2} </span></td>"
                + "<td align = \"right\" width =\"15\"></td></tr>"
                + "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; - No Claim Discount (NCD 45 %) </span></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:11px; line-height:18px\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {NCD} </span></td>"
                + "<td align = \"right\" width =\"15\"></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\">&nbsp; &nbsp;</span></td>"
                + "<td align = \"right\" width =\"15\"></td></tr>"
                + "<tr><td align = \"right\" width =\"15\"></td><td align = \"right\">&nbsp;</td>"
                + "<td align = \"right\">&nbsp;</td>"
                + "<td align = \"right\" width =\"15\"></td></tr>"
                + "<tr style = \"color:#000000\">"
                + "<td align =\"left\" width =\"15\"></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:11px; line-height:18px\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"></span>"
                + "<span style = \"font-family:quicksand; font-size:11px;font-weight:bold\"> Selected Add Ons:</span></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:11px; line-height:18px\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp;</span></td>"
                + "<td align = \"right\" width =\"15\"></td></tr>";

            if (salesReceiptContentDto.AddOns1.Length > 0)
            {
                EmailBody += "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold; display: block;\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px; display: block;\"> &nbsp; {AddOns1}</span></td>"
                + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:11px;line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {AddOns1Price} </span></td>"
                + "<td align = \"right\" width =\"15\"></td></tr><tr>";
            }
            if (salesReceiptContentDto.AddOns2.Length > 0)
            {
                EmailBody += "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold; display: block;\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px; display: block;\"> &nbsp; {AddOns2}</span></td>"
                + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:11px;line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {AddOns2Price} </span></td>"
                + "<td align = \"right\" width =\"15\"></td></tr><tr>";
            }
            if (salesReceiptContentDto.AddOns3.Length > 0)
            {
                EmailBody += "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold; display: block;\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px; display: block;\"> &nbsp; {AddOns3}</span></td>"
                + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:11px;line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {AddOns3Price} </span></td>"
                + "<td align = \"right\" width =\"15\"></td></tr><tr>";
            }
            if (salesReceiptContentDto.AddOns4.Length > 0)
            {
                EmailBody += "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold; display: block;\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px; display: block;\"> &nbsp; {AddOns4}</span></td>"
                + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:11px;line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {AddOns4Price} </span></td>"
                + "<td align = \"right\" width =\"15\"></td></tr><tr>";
            }
            if (salesReceiptContentDto.AddOns5.Length > 0)
            {
                EmailBody += "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold; display: block;\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px; display: block;\"> &nbsp; {AddOns5}</span></td>"
                + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:11px;line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {AddOns5Price} </span></td>"
                + "<td align = \"right\" width =\"15\"></td></tr><tr>";
            }
            if (salesReceiptContentDto.AddOns6.Length > 0)
            {
                EmailBody += "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold; display: block;\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px; display: block;\"> &nbsp; {AddOns6}</span></td>"
                + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:11px;line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {AddOns6Price} </span></td>"
                + "<td align = \"right\" width =\"15\"></td></tr><tr>";
            }
            if (salesReceiptContentDto.AddOns7.Length > 0)
            {
                EmailBody += "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold; display: block;\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px; display: block;\"> &nbsp; {AddOns7}</span></td>"
                + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:11px;line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {AddOns7Price} </span></td>"
                + "<td align = \"right\" width =\"15\"></td></tr><tr>";
            }
            if (salesReceiptContentDto.AddOns8.Length > 0)
            {
                EmailBody += "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold; display: block;\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px; display: block;\"> &nbsp; {AddOns8}</span></td>"
                + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:11px;line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {AddOns8Price} </span></td>"
                + "<td align = \"right\" width =\"15\"></td></tr><tr>";
            }
            if (salesReceiptContentDto.AddOns9.Length > 0)
            {
                EmailBody += "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold; display: block;\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px; display: block;\"> &nbsp; {AddOns9}</span></td>"
                + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:11px;line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {AddOns9Price} </span></td>"
                + "<td align = \"right\" width =\"15\"></td></tr><tr>";
            }
            if (salesReceiptContentDto.AddOns10.Length > 0)
            {
                EmailBody += "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold; display: block;\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px; display: block;\"> &nbsp; {AddOns10}</span></td>"
                + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:11px;line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {AddOns10Price} </span></td>"
                + "<td align = \"right\" width =\"15\"></td></tr><tr>";
            }
            if (salesReceiptContentDto.AddOns11.Length > 0)
            {
                EmailBody += "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold; display: block;\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px; display: block;\"> &nbsp; {AddOns11}</span></td>"
                + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:11px;line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {AddOns11Price} </span></td>"
                + "<td align = \"right\" width =\"15\"></td></tr><tr>";
            }
            if (salesReceiptContentDto.AddOns12.Length > 0)
            {
                EmailBody += "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold; display: block;\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px; display: block;\"> &nbsp; {AddOns12}</span></td>"
                + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:11px;line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {AddOns12Price} </span></td>"
                + "<td align = \"right\" width =\"15\"></td></tr><tr>";
            }
            if (salesReceiptContentDto.AddOns13.Length > 0)
            {
                EmailBody += "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold; display: block;\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px; display: block;\"> &nbsp; {AddOns13}</span></td>"
                + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:11px;line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {AddOns13Price} </span></td>"
                + "<td align = \"right\" width =\"15\"></td></tr><tr>";
            }
            if (salesReceiptContentDto.AddOns14.Length > 0)
            {
                EmailBody += "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold; display: block;\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px; display: block;\"> &nbsp; {AddOns14}</span></td>"
                + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:11px;line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {AddOns14Price} </span></td>"
                + "<td align = \"right\" width =\"15\"></td></tr><tr>";
            }
            if (salesReceiptContentDto.AddOns15.Length > 0)
            {
                EmailBody += "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold; display: block;\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px; display: block;\"> &nbsp; {AddOns15}</span></td>"
                + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:11px;line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {AddOns15Price} </span></td>"
                + "<td align = \"right\" width =\"15\"></td></tr><tr>";
            }


            EmailBody += "<td align = \"right\" width =\"15\"></td><td align = \"right\">&nbsp;</td>"
                + "<td align = \"right\"> &nbsp;</td>"
                + "<td align = \"right\" width =\"15\"></td></tr>"
                + "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:11px; line-height:18px\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"></span>"
                + "<span style = \"font-family:quicksand; font-size:11px; font-weight:bold\"> Gross Premium </span></td>"
                + "<td align = \"left\" style = \"font-family:quicksand; font-size:11px; line-height:18px\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {GrossPremium} </span></td>"
                + "<td align = \"right\" width =\"15\"></td></tr>"
                + "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; Server Tax @ 6 %</span></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px;font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {ServiceTax} </span></td>"
                + "<td align = \"right\" width =\"15\"></td></tr>"
                + "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; Stamp Duty </span></td>"
                + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px;font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {StampDuty} </span></td>"
                + "<td align = \"right\" width =\"15\"></td></tr>"
                + "<tr>"
                + "<td align = \"right\" width =\"15\"></td>"
                + "<td align = \"right\">&nbsp;</td>"
                + "<td align = \"right\">&nbsp;</td>"
                + "<td align = \"right\" width =\"15\"></td>"
                + "</tr>"
                + "<tr style = \"color:#000000\">"
                + "<td align =\"left\" width =\"15\"></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; Admin Fee </span></td>"
                + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {AdminFee} </span></td>"
                + "<td align = \"right\" width =\"15\"></td></tr>"
                + "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; MyEG + Delivery </span></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {MyegDelivery} </span></td>"
                + "<td align = \"right\" width =\"15\"></td></tr>"
                + "<tr style = \"color:#000000\"><td align =\"left\"width =\"15\"></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; Roadtax Renewal </span></td>"
                + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px;font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:11px;line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {RoadTaxRenewal} </span></td>"
                + "<td align = \"right\" width =\"15\"></td></tr>"
                + "<tr><td height = \"10px\" align =\"left\"></td>"
                + "<td height = \"10px\" colspan =\"2\" align =\"left\"></td>"
                + "<td height = \"10px\" align =\"left\"></td></tr>"
                + "<tr><td height = \"10px\" align =\"left\"></td>"
                + "<td height = \"10px\" colspan =\"2\" align =\"left\" style =\"border-top:1px dashed #9e9e9e\"></td>"
                + "<td height = \"10px\" align =\"left\"></td></tr>"
                + "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"></span>"
                + "<span style = \"font-family:quicksand; font-size:11px; font-weight:bold\"> Total Payable Premium </span></td>"
                + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {TotalPayablePremium} </span></td>"
                + "<td align = \"right\" width =\"15\"></td></tr></tbody>"
                + "</table></td></tr>"
                + "<tr><td align = \"left\" valign =\"top\">"
                + "<table border = \"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"100%\">"
                + "<tbody><tr><td align = \"left\" style=\"font-family:quicksand; font-size:15px; font-weight:bold;\"></td></tr></tbody>"
                + "</table></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody>"
                + "</table></td></tr></tbody></table><table border = \"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">"
                + "<tbody><tr><td height = \"20\"></td></tr></tbody>"
                + "</table></td><td valign=\"top\" width=\"45\">"
                + "</td></tr></tbody></table></td></tr></tbody></table></html>";


            string filepath = "";

            EmailBody = EmailBody.Replace(@"{VehicleOwnerName}", salesReceiptContentDto.VehicleOwnerName)
                .Replace(@"{VehicleICNumber}", salesReceiptContentDto.VehicleICNumber)
                .Replace(@"{VehicleOwnerAddress}", salesReceiptContentDto.VehicleOwnerAddress)
                .Replace(@"{VehicleRegistrationNumber}", salesReceiptContentDto.VehicleRegistrationNumber)
                .Replace(@"{Insurer}", salesReceiptContentDto.Insurer)
                .Replace(@"{InvoiceNo}", salesReceiptContentDto.VehicleRegistrationNumber)
                .Replace(@"{CoveragePeriod}", salesReceiptContentDto.CoveragePeriod)
                .Replace(@"{TypeOfCover}", salesReceiptContentDto.TypeOfCover)
                .Replace(@"{SumInsured}", salesReceiptContentDto.SumInsured)             
                .Replace(@"{TransactionDate}", salesReceiptContentDto.TransactionDate)
                .Replace(@"{CardHolderName}", salesReceiptContentDto.CardHolderName)
                .Replace(@"{AuthCode}", salesReceiptContentDto.AuthCode)
                .Replace(@"{PaymentMethod}", salesReceiptContentDto.PaymentMethod)
                .Replace(@"{BasicPremium}", salesReceiptContentDto.BasicPremium)
                .Replace(@"{loading1}", salesReceiptContentDto.loading1)
                .Replace(@"{loading2}", salesReceiptContentDto.loading2)
                .Replace(@"{NCD}", salesReceiptContentDto.NCD)
                .Replace(@"{AddOns1}", salesReceiptContentDto.AddOns1)
                .Replace(@"{AddOns1Price}", salesReceiptContentDto.AddOns1Price)
                .Replace(@"{AddOns2}", salesReceiptContentDto.AddOns2)
                .Replace(@"{AddOns2Price}", salesReceiptContentDto.AddOns2Price)
                .Replace(@"{AddOns3}", salesReceiptContentDto.AddOns3)
                .Replace(@"{AddOns3Price}", salesReceiptContentDto.AddOns3Price)
                .Replace(@"{AddOns4}", salesReceiptContentDto.AddOns4)
                .Replace(@"{AddOns4Price}", salesReceiptContentDto.AddOns4Price)
                .Replace(@"{AddOns5}", salesReceiptContentDto.AddOns5)
                .Replace(@"{AddOns5Price}", salesReceiptContentDto.AddOns5Price)
                .Replace(@"{AddOns6}", salesReceiptContentDto.AddOns6)
                .Replace(@"{AddOns6Price}", salesReceiptContentDto.AddOns6Price)
                .Replace(@"{AddOns7}", salesReceiptContentDto.AddOns7)
                .Replace(@"{AddOns7Price}", salesReceiptContentDto.AddOns7Price)
                .Replace(@"{AddOns8}", salesReceiptContentDto.AddOns8)
                .Replace(@"{AddOns8Price}", salesReceiptContentDto.AddOns8Price)
                .Replace(@"{AddOns9}", salesReceiptContentDto.AddOns9)
                .Replace(@"{AddOns9Price}", salesReceiptContentDto.AddOns9Price)
                .Replace(@"{AddOns10}", salesReceiptContentDto.AddOns10)
                .Replace(@"{AddOns10Price}", salesReceiptContentDto.AddOns10Price)
                .Replace(@"{AddOns11}", salesReceiptContentDto.AddOns11)
                .Replace(@"{AddOns11Price}", salesReceiptContentDto.AddOns11Price)
                .Replace(@"{AddOns12}", salesReceiptContentDto.AddOns12)
                .Replace(@"{AddOns12Price}", salesReceiptContentDto.AddOns12Price)
                .Replace(@"{AddOns13}", salesReceiptContentDto.AddOns13)
                .Replace(@"{AddOns13Price}", salesReceiptContentDto.AddOns13Price)
                .Replace(@"{AddOns14}", salesReceiptContentDto.AddOns14)
                .Replace(@"{AddOns14Price}", salesReceiptContentDto.AddOns14Price)
                .Replace(@"{AddOns15}", salesReceiptContentDto.AddOns15)
                .Replace(@"{AddOns15Price}", salesReceiptContentDto.AddOns15Price)
                .Replace(@"{GrossPremium}", salesReceiptContentDto.GrossPremium)
                .Replace(@"{ServiceTax}", salesReceiptContentDto.ServiceTax)
                .Replace(@"{StampDuty}", salesReceiptContentDto.StampDuty)
                .Replace(@"{AdminFee}", salesReceiptContentDto.AdminFee)
                .Replace(@"{MyegDelivery}", salesReceiptContentDto.MyegDelivery)
                .Replace(@"{RoadTaxRenewal}", salesReceiptContentDto.RoadTaxRenewal)
                .Replace(@"{TotalPayablePremium}", salesReceiptContentDto.TotalPayablePremium)
                .Replace(@"{LogoImg}", "https://system.carly.com.my/CarlyImage/carly-logo.png");

            Emails.IEmailAppService emailAppService = new Emails.EmailAppService(SettingManager);

            bool isEmailSent = await emailAppService.SendEmailAsync(salesReceiptContentDto.emailTo, EmailSubject, EmailBody, filepath);

            return isEmailSent;
        }

    }
}





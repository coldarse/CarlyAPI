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

        public UserAppService(
            IRepository<User, long> repository,
            UserManager userManager,
            RoleManager roleManager,
            IRepository<Role> roleRepository,
            IPasswordHasher<User> passwordHasher,
            IAbpSession abpSession,
            LogInManager logInManager)
            : base(repository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
            _abpSession = abpSession;
            _logInManager = logInManager;
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
            string EmailBody = "<div style=\"margin: 0\">"
                + "<table border=\"0\" cellspacing=\"0\" width=\"100%\" style=\"background:#fff; font-family:'Gotham','Hiragino Sans GB','Microsoft Yahei',sans-serif; font-size:18px; line-height:24px\">"
                + "<tbody>"
                + "<tr><td></td>"
                + "<td width=\"600\" style=\"background:#21bcc1; background:transparent linear-gradient(0deg,#21bcc1 0%,#052375 100%) 0% 0% no-repeat padding-box; border-radius: 0 0 1rem 1rem\">"
                + "<p style=\"text-align:center; margin: 15px 0\"><img src=\"{LogoImg}\" alt=\"Carly\" width=\"158\" height=\"73\" data-image-whitelisted=\"\" class=\"CToWUd\"></p>"
                + "<p style=\"text-align:center; font-size:45px; line-height:1; margin: 15px 0 25px; color:#fff;\">Affordable Motor Insurance </p>"
                + "</td>"
                + "<td></td></tr>"
                + "<tr><td></td>"
                + "<td>"
                + "<p style=\"text-align:center; font-size:20px; line-height:1; margin: 15px 0 5px; color:#000;\">Thank you for inquiring through us! </p>"
                + "<p style=\"text-align:center; font-size:15px; line-height:1; margin: 15px 0 1px; color:#000;\">Below are your quotation details.</p>"
                + "<tr><td></td>"
                + "<td width=\"600\">"
                + "<p style=\"margin: 20px 0\"></p>"
                + "<div>"
                + "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"max-width:580px; background-color:#f5f5f5; margin-left:10px; border:1px solid #d4d4d4; box-sizing:border-box; padding:10px 20px; border-radius: 0.5rem\">"
                + "<tbody>"
                + "<tr>"
                + "<td valign=\"top\" width=\"290\" style=\"width:290px; font-size:16px; border-bottom:1px solid #d4d4d4; padding:6px 0\">Vehicle Owner</td>"
                + "<td valign=\"top\" width=\"290\" style=\"width:290px; font-size:16px; border-bottom:1px solid #d4d4d4; padding:6px 0\">{VehicleOwnerName}</td>"
                + "</tr>"
                + "<tr>"
                + "<td valign=\"top\" width=\"290\" style=\"width:290px; font-size:16px; border-bottom:1px solid #d4d4d4; padding:6px 0\">Vehicle Registration Number</td>"
                + "<td valign=\"top\" width=\"290\" style=\"width:290px; font-size:16px; border-bottom:1px solid #d4d4d4; padding:6px 0\">{VehicleRegistrationNumber}</td>"
                + "</tr>"
                + "<tr>"
                + "<td valign=\"top\" width=\"290\" style=\"width:290px; font-size:16px; border-bottom:1px solid #d4d4d4; padding:6px 0\">Coverage Period</td>"
                + "<td valign=\"top\" width=\"290\" style=\"width:290px; font-size:16px; border-bottom:1px solid #d4d4d4; padding:6px 0\">{CoveragePeriod}</td>"
                + "</tr>"
                + "<tr>"
                + "<td valign=\"top\" width=\"290\" style=\"width:290px; font-size:16px; padding:6px 0\">Add Ons</td>"
                + "<td valign=\"top\" width=\"290\" style=\"width:290px; font-size:16px; padding:6px 0\">{AddOns}</td>"
                + "</tr>"
                + "</tbody>"
                + "</table>"
                + "</div>"
                + "</td>"
                + "<td></td></tr>"
                + "<tr><td></td>"
                + "<td width=\"600\">"
                + "<p style=\"text-align:center; margin: 10px 0 5px;\"><img src=\"{ImageLink}\" alt=\"Insurance\" style=\"max-width: 40%\" data-image-whitelisted=\"\" class=\"CToWUd\"></p>"
                + "<p style=\"font-size:16px; text-align:center; line-height:1.5; margin:0;\">Lowest Pricing</p>"
                + "<p style=\"font-size:30px; text-align:center; line-height:1; margin: 5px 0 5px;\">{Price}</p>"
                + "<p style=\"font-size:16px; text-align:center; line-height:1; margin: 0 0 25px;\"></p>"
                + "</td>"
                + "<td></td></tr>"
                + "<tr><td></td>"
                + "<td width=\"600\" style=\"background:#21bcc1; background:transparent linear-gradient(0deg,#052375 0%,#21bcc1 100%) 0% 0% no-repeat padding-box; padding: 35px 0px; border-radius: 1rem 1rem 0 0\">"
                + "<div>"
                + "<p style=\"text-align:center; margin: 0 0 25px;\"><a href=\"{ViewQuoteLink}\" style=\"background-color:#000; color:#fff; font-size:16px; padding:18px 35px; text-decoration:none; border-radius:35px; display:inline-block; line-height:1\" "
                + "target=\"_blank\" data-saferedirecturl=\"\"> View Quotes </a></p>"
                + "<p style=\"text-align:center; color:#fff\"> Pay With </p>"
                + "</div>"
                + "</tbody>"
                + "</table>"
                + "</div>";



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
                .Replace(@"{ViewQuoteLink}", emailContentDto.ViewQuoteLink);

            Emails.IEmailAppService emailAppService = new Emails.EmailAppService(SettingManager);

            return await emailAppService.SendEmailAsync(emailContentDto.emailTo, EmailSubject, EmailBody, filepath);
        }
    }
}


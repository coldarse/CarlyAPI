using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.MultiTenancy;
using Abp.Runtime.Security;
using Abp.UI;
using Carly.Authentication.External;
using Carly.Authentication.JwtBearer;
using Carly.Authorization;
using Carly.Authorization.Users;
using Carly.Models.TokenAuth;
using Carly.MultiTenancy;
using Carly.Packages;
using Carly.CustomerPrincipals;
using Carly.CustomerAddOns;
using Abp.Domain.Repositories;
using Carly.Vouchers.Dto;
using Carly.Vouchers;
using Carly.AddOns;
using Carly.Principals;
using Carly.AddOns.Dto;

namespace Carly.Controllers
{
    [Route("api/[controller]/[action]")]
    public class TokenAuthController : CarlyControllerBase
    {
        private readonly LogInManager _logInManager;
        private readonly ITenantCache _tenantCache;
        private readonly AbpLoginResultTypeHelper _abpLoginResultTypeHelper;
        private readonly TokenAuthConfiguration _configuration;
        private readonly IExternalAuthConfiguration _externalAuthConfiguration;
        private readonly IExternalAuthManager _externalAuthManager;
        private readonly UserRegistrationManager _userRegistrationManager;

        private readonly IRepository<Package> _PackageRepository;
        private readonly IRepository<CustomerPrincipal> _CustomerPrincipalRepository;
        private readonly IRepository<CustomerAddOn> _CustomerAddOnRepository;

        private readonly IRepository<Voucher> _VoucherRepository;

        private readonly IRepository<AddOn> _AddOnRepository;
        private readonly IRepository<Principal> _PrincipalRepository;

        public TokenAuthController(
            LogInManager logInManager,
            ITenantCache tenantCache,
            AbpLoginResultTypeHelper abpLoginResultTypeHelper,
            TokenAuthConfiguration configuration,
            IExternalAuthConfiguration externalAuthConfiguration,
            IExternalAuthManager externalAuthManager,
            UserRegistrationManager userRegistrationManager,
            IRepository<Package> PackageRepository,
            IRepository<CustomerPrincipal> CustomerPackageRepository,
            IRepository<CustomerAddOn> CustomerAddOnRepository,
            IRepository<Voucher> VoucherRepository,
            IRepository<AddOn> AddOnRepository,
            IRepository<Principal> PrincipalRepository)
        {
            _logInManager = logInManager;
            _tenantCache = tenantCache;
            _abpLoginResultTypeHelper = abpLoginResultTypeHelper;
            _configuration = configuration;
            _externalAuthConfiguration = externalAuthConfiguration;
            _externalAuthManager = externalAuthManager;
            _userRegistrationManager = userRegistrationManager;
            _PackageRepository = PackageRepository;
            _CustomerPrincipalRepository = CustomerPackageRepository;
            _CustomerAddOnRepository = CustomerAddOnRepository;
            _VoucherRepository = VoucherRepository;
            _AddOnRepository = AddOnRepository;
            _PrincipalRepository = PrincipalRepository;
        }

        [HttpGet]
        public Package GetPackageById(int id)
        {
            List<CustomerPrincipal> tempPrincipal = _CustomerPrincipalRepository.GetAll().Where(f => f.PackageId.ToString().Equals(id.ToString())).ToList();

            //int principalId = tempPrincipal[0].Id;

            List<CustomerAddOn>[] a = new List<CustomerAddOn>[3];
            int x = 0;
            foreach (var prin in tempPrincipal)
            {
                a[x] = _CustomerAddOnRepository.GetAll().Where(f => f.CustomerPrincipalId.ToString().Equals(prin.Id.ToString())).ToList();
                x += 1;
            }
            //List<CustomerAddOn> tempAddOn = _CustomerAddOnRepository.GetAll().Where(f => f.CustomerPrincipalId.ToString().Equals(principalId.ToString())).ToList();

            Package tempPackage = _PackageRepository.FirstOrDefault(f => f.Id.ToString().Equals(id.ToString()));

            return tempPackage;
        }

        [HttpPost]
        public isValidDto isVoucherValid(string vouchercode)
        {
            List<Voucher> tempVoucher = _VoucherRepository.GetAll().ToList();

            isValidDto tempIsValid = new isValidDto();

            foreach (var d in tempVoucher)
            {
                if (vouchercode.ToLower().Contains(d.code.ToLower().ToString()))
                {
                    string temp = vouchercode.ToUpper().Replace(d.code.ToUpper().ToString(), "");
                    int limit = 0;
                    bool isSucceed = Int32.TryParse(temp, out limit);
                    if (isSucceed)
                    {
                        if (limit <= Convert.ToInt32(d.limit))
                        {
                            tempIsValid.isValid = true;
                            tempIsValid.Type = d.type;
                            tempIsValid.minAmount = d.minAmount;
                            tempIsValid.discountAmount = d.discountAmount;
                            tempIsValid.giftId = d.giftId;

                            return tempIsValid;
                        }
                        else
                        {
                            tempIsValid.isValid = false;
                            tempIsValid.Type = "";
                            tempIsValid.minAmount = 0.00f;
                            tempIsValid.discountAmount = 0.00f;
                            tempIsValid.giftId = 0;

                            return tempIsValid;
                        }
                    }
                    else
                    {
                        continue;
                    }

                }
            }

            tempIsValid.isValid = false;
            tempIsValid.Type = "";
            tempIsValid.minAmount = 0.00f;
            tempIsValid.discountAmount = 0.00f;
            tempIsValid.giftId = 0;

            return tempIsValid;
        }

        [HttpGet]
        public GiftDto getGift(int giftId)
        {
            AddOn tempAddOn = _AddOnRepository.FirstOrDefault(giftId);
            Principal tempPrincipal = _PrincipalRepository.FirstOrDefault(tempAddOn.PrincipalId);

            GiftDto tempGift = new GiftDto();

            tempGift.addOnId = tempAddOn.Id;
            tempGift.addOnName = tempAddOn.addonname;
            tempGift.principalId = tempPrincipal.Id;
            tempGift.principalName = tempPrincipal.Name;

            return tempGift;
        } 

        [HttpPost]
        public async Task<AuthenticateResultModel> Authenticate([FromBody] AuthenticateModel model)
        {
            var loginResult = await GetLoginResultAsync(
                model.UserNameOrEmailAddress,
                model.Password,
                GetTenancyNameOrNull()
            );

            var accessToken = CreateAccessToken(CreateJwtClaims(loginResult.Identity));

            return new AuthenticateResultModel
            {
                AccessToken = accessToken,
                EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds,
                UserId = loginResult.User.Id
            };
        }

        [HttpGet]
        public List<ExternalLoginProviderInfoModel> GetExternalAuthenticationProviders()
        {
            return ObjectMapper.Map<List<ExternalLoginProviderInfoModel>>(_externalAuthConfiguration.Providers);
        }

        [HttpPost]
        public async Task<ExternalAuthenticateResultModel> ExternalAuthenticate([FromBody] ExternalAuthenticateModel model)
        {
            var externalUser = await GetExternalUserInfo(model);

            var loginResult = await _logInManager.LoginAsync(new UserLoginInfo(model.AuthProvider, model.ProviderKey, model.AuthProvider), GetTenancyNameOrNull());

            switch (loginResult.Result)
            {
                case AbpLoginResultType.Success:
                    {
                        var accessToken = CreateAccessToken(CreateJwtClaims(loginResult.Identity));
                        return new ExternalAuthenticateResultModel
                        {
                            AccessToken = accessToken,
                            EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                            ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds
                        };
                    }
                case AbpLoginResultType.UnknownExternalLogin:
                    {
                        var newUser = await RegisterExternalUserAsync(externalUser);
                        if (!newUser.IsActive)
                        {
                            return new ExternalAuthenticateResultModel
                            {
                                WaitingForActivation = true
                            };
                        }

                        // Try to login again with newly registered user!
                        loginResult = await _logInManager.LoginAsync(new UserLoginInfo(model.AuthProvider, model.ProviderKey, model.AuthProvider), GetTenancyNameOrNull());
                        if (loginResult.Result != AbpLoginResultType.Success)
                        {
                            throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(
                                loginResult.Result,
                                model.ProviderKey,
                                GetTenancyNameOrNull()
                            );
                        }

                        return new ExternalAuthenticateResultModel
                        {
                            AccessToken = CreateAccessToken(CreateJwtClaims(loginResult.Identity)),
                            ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds
                        };
                    }
                default:
                    {
                        throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(
                            loginResult.Result,
                            model.ProviderKey,
                            GetTenancyNameOrNull()
                        );
                    }
            }
        }

        private async Task<User> RegisterExternalUserAsync(ExternalAuthUserInfo externalUser)
        {
            var user = await _userRegistrationManager.RegisterAsync(
                externalUser.Name,
                externalUser.Surname,
                externalUser.EmailAddress,
                externalUser.EmailAddress,
                Authorization.Users.User.CreateRandomPassword(),
                true
            );

            user.Logins = new List<UserLogin>
            {
                new UserLogin
                {
                    LoginProvider = externalUser.Provider,
                    ProviderKey = externalUser.ProviderKey,
                    TenantId = user.TenantId
                }
            };

            await CurrentUnitOfWork.SaveChangesAsync();

            return user;
        }

        private async Task<ExternalAuthUserInfo> GetExternalUserInfo(ExternalAuthenticateModel model)
        {
            var userInfo = await _externalAuthManager.GetUserInfo(model.AuthProvider, model.ProviderAccessCode);
            if (userInfo.ProviderKey != model.ProviderKey)
            {
                throw new UserFriendlyException(L("CouldNotValidateExternalUser"));
            }

            return userInfo;
        }

        private string GetTenancyNameOrNull()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                return null;
            }

            return _tenantCache.GetOrNull(AbpSession.TenantId.Value)?.TenancyName;
        }

        private async Task<AbpLoginResult<Tenant, User>> GetLoginResultAsync(string usernameOrEmailAddress, string password, string tenancyName)
        {
            var loginResult = await _logInManager.LoginAsync(usernameOrEmailAddress, password, tenancyName);

            switch (loginResult.Result)
            {
                case AbpLoginResultType.Success:
                    return loginResult;
                default:
                    throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(loginResult.Result, usernameOrEmailAddress, tenancyName);
            }
        }

        private string CreateAccessToken(IEnumerable<Claim> claims, TimeSpan? expiration = null)
        {
            var now = DateTime.UtcNow;

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _configuration.Issuer,
                audience: _configuration.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(expiration ?? _configuration.Expiration),
                signingCredentials: _configuration.SigningCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }

        private static List<Claim> CreateJwtClaims(ClaimsIdentity identity)
        {
            var claims = identity.Claims.ToList();
            var nameIdClaim = claims.First(c => c.Type == ClaimTypes.NameIdentifier);

            // Specifically add the jti (random nonce), iat (issued timestamp), and sub (subject/user) claims.
            claims.AddRange(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, nameIdClaim.Value),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            });

            return claims;
        }

        private string GetEncryptedAccessToken(string accessToken)
        {
            return SimpleStringCipher.Instance.Encrypt(accessToken, AppConsts.DefaultPassPhrase);
        }
    }
}

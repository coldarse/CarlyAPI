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
using Carly.EncryptKeys;
using Newtonsoft.Json;
using Carly.Sales;
using Carly.Sales.Dto;

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

        private readonly IRepository<GeneratedVoucher> _GeneratedVoucherRepository;
        private readonly IRepository<Sale> _SaleRepository;

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
            IRepository<Principal> PrincipalRepository,
            IRepository<GeneratedVoucher> GeneratedVoucherRepository,
            IRepository<Sale> SaleRepository
            )
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
            _GeneratedVoucherRepository = GeneratedVoucherRepository;
            _SaleRepository = SaleRepository;
        }

        [HttpGet]
        public bool GetSale(string getSale)
        {

            getSaleDto tempSale = JsonConvert.DeserializeObject<getSaleDto>(getSale);
            string stringid = EncryptKey.Decrypt(tempSale.id);
            string signature = tempSale.signature.Replace(" ", "+");

            tempSale.signature = "";

            string JSONString = JsonConvert.SerializeObject(tempSale, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
            });

            string encryptedString = EncryptKey.Encrypt(JSONString);

            if (!Equals(encryptedString, signature)) { return false; }


            List<Sale> tempListSale = _SaleRepository.GetAll().ToList();

            foreach(var ts in tempListSale)
            {
                if(ts.Package.ToString() == stringid)
                {
                    return false;
                }
            }

            return true;
        }


        [HttpPost]
        public async Task<Sale> CreateSale(string sales)
        {
            createSalesDto tempsales = JsonConvert.DeserializeObject<createSalesDto>(sales);
            string package = tempsales.package;
            int selectedPrincipal = tempsales.selectedPrincipal;
            string selectedAddOns = tempsales.selectedAddOns;
            string premium = tempsales.premium;
            string address1 = tempsales.address1;
            string address2 = tempsales.address2;
            string postcode = tempsales.postcode;
            string city = tempsales.city;
            string state = tempsales.state;
            string signature = tempsales.signature.Replace(" ", "+");
            string claimedvoucher = tempsales.claimedvoucher;

            int tempPackage = Convert.ToInt32(EncryptKey.Decrypt(package));

            tempsales.signature = "";

            string JSONString = JsonConvert.SerializeObject(tempsales, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
            });

            string encryptedString = EncryptKey.Encrypt(JSONString);

            if (!Equals(encryptedString, signature)) { return new Sale(); }

            Sale newSales = new Sale();
            newSales.Package = tempPackage;
            newSales.SelectedPrincipal = selectedPrincipal;
            newSales.SelectedAddOns = selectedAddOns;
            newSales.Premium = float.Parse(premium);
            newSales.TransactionDateTime = DateTime.Now;
            newSales.Address1 = address1;
            newSales.Address2 = address2;
            newSales.Postcode = postcode;
            newSales.City = city;
            newSales.State = state;
            newSales.ClaimedVoucher = claimedvoucher;

            return await _SaleRepository.InsertAsync(newSales);
        }

        [HttpPut]
        public async Task<bool> RedeemVoucher(string redeemvoucher)
        {
            RedeemVoucherDto tempredeemvoucher = JsonConvert.DeserializeObject<RedeemVoucherDto>(redeemvoucher);
            string vouchercode = tempredeemvoucher.vouchercode;
            string packageid = EncryptKey.Decrypt(tempredeemvoucher.packageid);
            string claimDate = tempredeemvoucher.claimDate;
            string signature = tempredeemvoucher.signature.Replace(" ", "+");

            tempredeemvoucher.signature = "";

            string JSONString = JsonConvert.SerializeObject(tempredeemvoucher, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
            });

            string encryptedString = EncryptKey.Encrypt(JSONString);

            if(!Equals(encryptedString, signature)) { return false; }

            DateTime newClaimDate = Convert.ToDateTime(claimDate);
            List<GeneratedVoucher> tempGenVoucher = _GeneratedVoucherRepository.GetAll().ToList();
            List<Package> tempPackage = _PackageRepository.GetAll().ToList();

            string regno = "";
            foreach(var p in tempPackage)
            {
                if(p.Id.ToString() == packageid)
                {
                    regno = p.VehicleRegNo;
                    break;
                }
            }

            List<Voucher> tempVoucher = _VoucherRepository.GetAll().ToList();
            bool isGeneral = false;
            foreach(var v in tempVoucher)
            {
                if (vouchercode.ToLower().Contains(v.code.ToLower()))
                {
                    if (v.description.ToLower().Contains("general"))
                    {
                        isGeneral = true;
                    }
                }
            }

            
            foreach (var g in tempGenVoucher)
            {
                if (g.Code.ToLower().Equals(vouchercode.ToLower()))
                {
                    if(newClaimDate >= g.StartDate && newClaimDate <= g.EndDate)
                    {
                        if(isGeneral == false)
                        {
                            g.isRedeemed = true;
                            g.RedeemedByPackage = regno;
                            await _GeneratedVoucherRepository.UpdateAsync(g);
                            return true;
                        }
                        else
                        {
                            return true;
                        }
                        
                    }
                }
            }

            return false;

        }


        [HttpPost]
        public string testEncrypt(int id)
        {
            string stringid = id.ToString();
            string encryptedID = EncryptKey.Encrypt(stringid);
            return encryptedID;
        }

        [HttpPost]
        public int testDecrypt(string encryptedID)
        {
            string stringid = EncryptKey.Decrypt(encryptedID);
            int id = Convert.ToInt32(stringid);
            return id;
        }

        [HttpGet]
        public Package GetPackageById(string getpackagebyid)
        {
            GetPackageByIdDto tempgetpackagebyid = JsonConvert.DeserializeObject<GetPackageByIdDto>(getpackagebyid);
            string stringid = EncryptKey.Decrypt(tempgetpackagebyid.id);
            string signature = tempgetpackagebyid.signature.Replace(" ", "+");

            tempgetpackagebyid.signature = "";

            string JSONString = JsonConvert.SerializeObject(tempgetpackagebyid, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
            });

            string encryptedString = EncryptKey.Encrypt(JSONString);

            if (!Equals(encryptedString, signature)) { return new Package(); }

            List<CustomerPrincipal> tempPrincipal = _CustomerPrincipalRepository.GetAll().Where(f => f.PackageId.ToString().Equals(stringid)).ToList();


            List<CustomerAddOn>[] a = new List<CustomerAddOn>[3];
            int x = 0;
            foreach (var prin in tempPrincipal)
            {
                a[x] = _CustomerAddOnRepository.GetAll().Where(f => f.CustomerPrincipalId.ToString().Equals(prin.Id.ToString())).ToList();
                x += 1;
            }
            //List<CustomerAddOn> tempAddOn = _CustomerAddOnRepository.GetAll().Where(f => f.CustomerPrincipalId.ToString().Equals(principalId.ToString())).ToList();

            Package tempPackage = _PackageRepository.FirstOrDefault(f => f.Id.ToString().Equals(stringid));

            return tempPackage;
        }

        [HttpPost]
        public isValidDto isVoucherValid(string isvouchervalid)
        {
            isVoucherValidDto tempisvouchervalid = JsonConvert.DeserializeObject<isVoucherValidDto>(isvouchervalid);
            string vouchercode = tempisvouchervalid.vouchercode;
            string claimDate = tempisvouchervalid.claimDate;
            string signature = tempisvouchervalid.signature.Replace(" ", "+");

            tempisvouchervalid.signature = "";

            string JSONString = JsonConvert.SerializeObject(tempisvouchervalid, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
            });

            string encryptedString = EncryptKey.Encrypt(JSONString);
                
            if (!Equals(encryptedString, signature)) { return new isValidDto(); }

            DateTime newclaimDate = Convert.ToDateTime(claimDate);
            isValidDto tempIsValid = new isValidDto();
            if (vouchercode == null)
            {
                tempIsValid.isValid = false;
                tempIsValid.Type = "";
                tempIsValid.minAmount = 0.00f;
                tempIsValid.discountAmount = 0.00f;
                tempIsValid.giftId = 0;
                tempIsValid.reason = "Empty Voucher Code";

                return tempIsValid;
            }

            List<GeneratedVoucher> tempGenVoucher = _GeneratedVoucherRepository.GetAll().ToList();

            foreach(var g in tempGenVoucher)
            {
                if (g.Code.ToLower().Equals(vouchercode.ToLower()))
                {
                    if(g.isRedeemed == true)
                    {
                        tempIsValid.isValid = false;
                        tempIsValid.Type = "";
                        tempIsValid.minAmount = 0.00f;
                        tempIsValid.discountAmount = 0.00f;
                        tempIsValid.giftId = 0;
                        tempIsValid.reason = "Voucher Code has been Claimed";

                        return tempIsValid;
                    }
                }
            }

            List<Voucher> tempVoucher = _VoucherRepository.GetAll().ToList();

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
                            if (newclaimDate >= d.startDate && newclaimDate <= d.stopDate)
                            {
                                tempIsValid.isValid = true;
                                tempIsValid.Type = d.type;
                                tempIsValid.minAmount = d.minAmount;
                                tempIsValid.discountAmount = d.discountAmount;
                                tempIsValid.giftId = d.giftId;
                                tempIsValid.reason = "Success";

                                return tempIsValid;
                            }
                            else
                            {
                                tempIsValid.isValid = false;
                                tempIsValid.Type = "";
                                tempIsValid.minAmount = 0.00f;
                                tempIsValid.discountAmount = 0.00f;
                                tempIsValid.giftId = 0;
                                tempIsValid.reason = "Voucher Expired/Not yet activated";

                                return tempIsValid;
                            }
                        }
                        else
                        {
                            tempIsValid.isValid = false;
                            tempIsValid.Type = "";
                            tempIsValid.minAmount = 0.00f;
                            tempIsValid.discountAmount = 0.00f;
                            tempIsValid.giftId = 0;
                            tempIsValid.reason = "Invalid Voucher Code";

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
            tempIsValid.reason = "Invalid Voucher Code";

            return tempIsValid;
        }

        [HttpGet]
        public GiftDto getGift(string getgift)
        {
            getGiftDto tempgetgift = JsonConvert.DeserializeObject<getGiftDto>(getgift);
            int giftId = tempgetgift.giftId;
            string signature = tempgetgift.signature.Replace(" ", "+");

            tempgetgift.signature = "";

            string JSONString = JsonConvert.SerializeObject(tempgetgift, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
            });

            string encryptedString = EncryptKey.Encrypt(JSONString);

            if (!Equals(encryptedString, signature)) { return new GiftDto(); }

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

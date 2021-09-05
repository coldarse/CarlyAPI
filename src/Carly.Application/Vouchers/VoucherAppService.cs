using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Carly.Authorization;
using Carly.Vouchers.Dto;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carly.Vouchers
{
    [AbpAuthorize(PermissionNames.Pages_Vouchers)]
    public class VoucherAppService : CrudAppService<Voucher, VoucherDto, int>
    {

        private readonly IRepository<Voucher> _VoucherRepository;
        private readonly IRepository<GeneratedVoucher> _GeneratedVoucherRepository;
        private static Random random = new Random();
        public VoucherAppService(IRepository<Voucher, int> repository,
            IRepository<Voucher> VoucherRepository,
            IRepository<GeneratedVoucher> GeneratedVoucherRepository) : base(repository)
        {
            _VoucherRepository = VoucherRepository;
            _GeneratedVoucherRepository = GeneratedVoucherRepository;
        }

        public override void Delete(EntityDto<int> input)
        {
            //get voucher
            Voucher tempVoucher = _VoucherRepository.FirstOrDefault(y => y.Id.ToString().Equals(input.Id.ToString()));
            //get list of all generated vouchers
            List<GeneratedVoucher> tempGenVoucher = _GeneratedVoucherRepository.GetAll().ToList();

            foreach(var vouch in tempGenVoucher)
            {
                if (vouch.Code.ToLower().Contains(tempVoucher.code.ToLower()))
                {
                    _GeneratedVoucherRepository.Delete(vouch.Id);
                }
            }

            base.Delete(input);
        }

        [HttpPut]
        public async Task<bool> RedeemVoucher(string vouchercode, int packageid, string claimDate)
        {
            DateTime newClaimDate = Convert.ToDateTime(claimDate);
            List<GeneratedVoucher> tempGenVoucher = _GeneratedVoucherRepository.GetAll().ToList();

            foreach (var g in tempGenVoucher)
            {
                if (g.Code.ToLower().Equals(vouchercode.ToLower()))
                {
                    if (newClaimDate >= g.StartDate && newClaimDate <= g.EndDate)
                    {
                        g.isRedeemed = true;
                        g.RedeemedByPackage = packageid.ToString();
                        await _GeneratedVoucherRepository.UpdateAsync(g);
                        return true;
                    }
                }
            }

            return false;

        }

        public async Task<bool> CreateNewVoucher(VoucherDto voucher)
        {
            List<Voucher> tempVoucher = _VoucherRepository.GetAll().ToList();

            int count = 0;

            foreach(var vouch in tempVoucher)
            {
                if (vouch.code.ToLower().Equals(voucher.code.ToLower()))
                {
                    count += 1;
                }
            }

            if(count == 0)
            {
                Voucher newVoucher = new Voucher();
                newVoucher.name = voucher.name;
                newVoucher.code = voucher.code;
                newVoucher.type = voucher.type;
                newVoucher.minAmount = voucher.minAmount;
                newVoucher.description = voucher.description;
                newVoucher.startDate = voucher.startDate;
                newVoucher.stopDate = voucher.stopDate;
                newVoucher.limit = voucher.limit;
                newVoucher.discountAmount = voucher.discountAmount;
                newVoucher.giftId = voucher.giftId;
                newVoucher.isGenerated = false;
                
                await _VoucherRepository.InsertAsync(newVoucher);
                return true;
            }
            else
            {
                return false;
            }
        }

        
        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [HttpPost]
        public async Task<bool> GenerateVouchers(int id) 
        {
            try
            {
                Voucher tempVoucher = _VoucherRepository.FirstOrDefault(id);
                int padno = tempVoucher.limit.ToString().Length;

                if (tempVoucher.description.ToLower().Contains("general"))
                {
                    string price = tempVoucher.minAmount.ToString();
                    GeneratedVoucher tempGenVoucher = new GeneratedVoucher();
                    tempGenVoucher.Code = tempVoucher.code.ToUpper() + price;
                    tempGenVoucher.StartDate = tempVoucher.startDate;
                    tempGenVoucher.EndDate = tempVoucher.stopDate;
                    tempGenVoucher.isRedeemed = false;
                    tempGenVoucher.Type = tempVoucher.type;

                    await _GeneratedVoucherRepository.InsertAsync(tempGenVoucher);

                    tempVoucher.isGenerated = true;
                    await _VoucherRepository.UpdateAsync(tempVoucher);

                    return true;

                }
                else
                {
                    for (int x = 1; x <= tempVoucher.limit; x++)
                    {
                        string rand = RandomString(2);
                        GeneratedVoucher tempGenVoucher = new GeneratedVoucher();
                        string paddedleft = x.ToString().ToUpper().PadLeft(padno, '0') + rand;
                        tempGenVoucher.Code = tempVoucher.code + paddedleft;
                        tempGenVoucher.StartDate = tempVoucher.startDate;
                        tempGenVoucher.EndDate = tempVoucher.stopDate;
                        tempGenVoucher.isRedeemed = false;
                        tempGenVoucher.Type = tempVoucher.type;

                        await _GeneratedVoucherRepository.InsertAsync(tempGenVoucher);
                    }

                    tempVoucher.isGenerated = true;
                    await _VoucherRepository.UpdateAsync(tempVoucher);

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        [HttpPost]
        public isValidDto isVoucherValid(string vouchercode, string claimDate)
        {
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

            foreach (var g in tempGenVoucher)
            {
                if (g.Code.ToLower().Equals(vouchercode.ToLower()))
                {
                    if (g.isRedeemed == true)
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
    }
}

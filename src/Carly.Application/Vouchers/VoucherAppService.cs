﻿using Abp.Application.Services;
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
    public class VoucherAppService : CrudAppService<Voucher, VoucherDto>
    {

        private readonly IRepository<Voucher> _VoucherRepository;
        private readonly IRepository<GeneratedVoucher> _GeneratedVoucherRepository;
        public VoucherAppService(IRepository<Voucher, int> repository,
            IRepository<Voucher> VoucherRepository,
            IRepository<GeneratedVoucher> GeneratedVoucherRepository) : base(repository)
        {
            _VoucherRepository = VoucherRepository;
            _GeneratedVoucherRepository = GeneratedVoucherRepository;
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
                        g.RedeemedByPackage = packageid;
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

        [HttpPost]
        public async Task<bool> GenerateVouchers(int id) 
        {
            try
            {
                Voucher tempVoucher = _VoucherRepository.FirstOrDefault(id);
                int padno = tempVoucher.limit.ToString().Length;

                for (int x = 1; x <= tempVoucher.limit; x++)
                {
                    GeneratedVoucher tempGenVoucher = new GeneratedVoucher();
                    string paddedleft = x.ToString().PadLeft(padno, '0');
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

using Abp.Application.Services;
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
        public VoucherAppService(IRepository<Voucher, int> repository, 
            IRepository<Voucher> VoucherRepository) : base(repository)
        {
            _VoucherRepository = VoucherRepository;
        }

        [HttpPost]
        public isValidDto isVoucherValid(string vouchercode)
        {
            List<Voucher> tempVoucher = _VoucherRepository.GetAll().ToList();

            isValidDto tempIsValid = new isValidDto();

            foreach(var d in tempVoucher)
            {
                if (vouchercode.ToLower().Contains(d.code.ToLower().ToString())){
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
    }
}

using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Carly.Authorization;
using Carly.GeneratedVouchers.Dto;
using Carly.Packages;
using Carly.Vouchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carly.GeneratedVouchers
{
    [AbpAuthorize(PermissionNames.Pages_GeneratedVouchers)]
    public class GeneratedVoucherAppService : CrudAppService<GeneratedVoucher, GeneratedVoucherDto, int, PagedGeneratedVoucherResultRequestDto>
    {
        private readonly IRepository<Package> _PackageRepository;
        public GeneratedVoucherAppService(IRepository<GeneratedVoucher, int> repository, IRepository<Package> PackageRepository) : base(repository)
        {
            _PackageRepository = PackageRepository;
        }

        

        //public PagedResultDto<GeneratedVoucherDisplayDto> GetAllDisplay(PagedGeneratedVoucherResultRequestDto input)
        //{
        //    PagedResultDto<GeneratedVoucherDto> tempList = base.GetAll(input);
        //    List<Package> tempPack = _PackageRepository.GetAll().ToList();
        //    int checkcount = 0;

        //    PagedResultDto<GeneratedVoucherDisplayDto> finalList = new PagedResultDto<GeneratedVoucherDisplayDto>();
        //    foreach(var c in tempList.Items)
        //    {
        //        foreach(var d in tempPack)
        //        {
        //            if(c.RedeemedByPackage == d.Id)
        //            {
        //                GeneratedVoucherDisplayDto tempfoo = new GeneratedVoucherDisplayDto();
        //                tempfoo.Code = c.Code;
        //                tempfoo.StartDate = c.StartDate;
        //                tempfoo.EndDate = c.EndDate;
        //                tempfoo.isRedeemed = c.isRedeemed;
        //                tempfoo.Type = c.Type;
        //                tempfoo.RedeemedByVehicleReg = d.VehicleRegNo;

        //                finalList.Items.ToList().Add(tempfoo);
        //                checkcount = 0;
        //            }
        //            else
        //            {
        //                checkcount += 1;
        //            }
        //        }
        //        if(checkcount == tempPack.Count)
        //        {
        //            GeneratedVoucherDisplayDto tempfoo = new GeneratedVoucherDisplayDto();
        //            tempfoo.Code = c.Code;
        //            tempfoo.StartDate = c.StartDate;
        //            tempfoo.EndDate = c.EndDate;
        //            tempfoo.isRedeemed = c.isRedeemed;
        //            tempfoo.Type = c.Type;
        //            tempfoo.RedeemedByVehicleReg = "";

        //            finalList.Items.ToList().Add(tempfoo);
        //            checkcount = 0;
        //        }
        //    }


        //    return finalList;
        //}

        protected override IQueryable<GeneratedVoucher> CreateFilteredQuery(PagedGeneratedVoucherResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Code.Contains(input.Keyword))
                .WhereIf(input.isRedeemed.HasValue, x => x.isRedeemed == input.isRedeemed);

        }

    }
}

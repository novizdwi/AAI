using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using Swashbuckle.AspNetCore.Annotations;
using WEB_API.Data;
using WEB_API.Models;

namespace WEB_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClaimApiController : ControllerBase
    {
        private readonly WEB_APIContext _context;
        public ClaimApiController(WEB_APIContext context)
        {
            _context = context;
        }

        [Route("Index")]
        [HttpGet]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, description: "List<ClaimModel>", type: typeof(List<ClaimModel>))]
        public ActionResult Index(string? status = null, string? DateFrom = null, string? DateTo = null)
        {
            //var Result = service.GetAll(SearchString);
            List<ClaimModel> vm = new List<ClaimModel>();
            var getAll = _context.ClaimModel.ToList();
            if (getAll != null)
            {
                if (!string.IsNullOrEmpty(status))
                {
                    getAll = getAll.Where(x => x.Status.ToLower().Contains(status.ToLower())).ToList();
                }
                if(DateFrom!= null && DateTo!= null)
                {
                    getAll = getAll.Where(x => x.DateOfService >= DateTime.Parse(DateFrom) && x.DateOfService <= DateTime.Parse(DateTo)).ToList();
                }
            }
            return Ok(getAll);
        }

        [Route("Add")]
        [HttpPost]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, description: "OperationResults", type: typeof(OperationResults))]
        public ActionResult Add(ClaimModel model)
        {
            OperationResults result = new OperationResults();

            try
            {
                using (var scope = new TransactionScope(
                    TransactionScopeOption.Required,
                    TimeSpan.FromMinutes(60),
                    TransactionScopeAsyncFlowOption.Enabled
                ))
                {
                    var data = new ClaimModel()
                    {
                        PatientName = model.PatientName,
                        DateOfService = model.DateOfService,
                        MedicalProvider = model.MedicalProvider,
                        ClaimAmount = model.ClaimAmount,
                        Diagnosis = model.Diagnosis,
                        Status = "Pending"
                    };

                    _context.ClaimModel.Add(data);
                    var success = _context.SaveChanges() > 0;
                    if (success)
                    {
                        scope.Complete();
                        return Ok();
                    }
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.ToString(), title:"Localhost");
            }

        }

        [Route("Update")]
        [HttpPost]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, description: "OperationResults", type: typeof(OperationResults))]
        public ActionResult Update(ClaimModel viewModel)
        {
            OperationResults result = new OperationResults();
            try
            {
                using (var scope = new TransactionScope(
                    TransactionScopeOption.Required,
                    TimeSpan.FromMinutes(60),
                    TransactionScopeAsyncFlowOption.Enabled
                ))
                {
                    var data = _context.ClaimModel.Find(viewModel.ClaimId);
                    if (data != null) { 
                        data.Status = viewModel.Status;

                        var success = _context.SaveChanges() > 0;
                        if (success)
                        {
                            scope.Complete();
                            return Ok();
                        }
                    }
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.ToString(), title: "Localhost");
            }

        }
        
        [Route("Delete")]
        [HttpPost]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, description: "OperationResults", type: typeof(OperationResults))]
        public  ActionResult Delete(int ClaimId)
        {
            OperationResults result = new OperationResults();
            try
            {
                using (var scope = new TransactionScope(
                    TransactionScopeOption.Required,
                    TimeSpan.FromMinutes(60),
                    TransactionScopeAsyncFlowOption.Enabled
                ))
                {
                    var data = _context.ClaimModel.Find(ClaimId);
                    if (data != null) {

                        _context.ClaimModel.Remove(data);

                        var success = _context.SaveChanges() > 0;
                        if (success)
                        {
                            scope.Complete();
                            return Ok();
                        }
                    }
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.ToString(), title: "Localhost");
            }

        }

        [Route("CheckExist")]
        [HttpGet]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, description: "int", type: typeof(int))]
        public ActionResult CheckExist(string patientName)
        {
            int result = 0;
            var getAll = _context.ClaimModel.Where(x=>x.PatientName == patientName && x.Status == "Pending");
            if (getAll != null)
                result = 1;

            return Ok(result);
        }

        [Route("GetById")]
        [HttpGet]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, description: "ClaimModel", type: typeof(ClaimModel))]
        public ActionResult CheckExist(int id)
        {
            var query = _context.ClaimModel.Where(s => s.ClaimId == id).FirstOrDefault();
            if (query != null)
            {
                ClaimModel model = new ClaimModel();

                model.ClaimId = query.ClaimId;
                model.PatientName = query.PatientName;
                model.DateOfService = query.DateOfService;
                model.MedicalProvider = query.MedicalProvider;
                model.ClaimAmount = query.ClaimAmount;
                model.Diagnosis = query.Diagnosis;
                model.Status = query.Status;

                return Ok(model);

            }
            return Ok(null);
        }

        [Route("ClaimStatuses")]
        [HttpGet]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, description: "List<SelectListItem>", type: typeof(List<SelectListItem>))]
        public ActionResult ClaimStatuses(string? Status = null)
        {
            var query = Enum.GetValues(typeof(StatusEnum)).Cast<StatusEnum>().Select(v => new SelectListItem
            {
                Text = v.ToString(),
                Value = v.ToString(),
                Selected = Status == v.ToString() ? true : false
            }).ToList();

            if (query != null)
            {
                return Ok(query);

            }
            return Ok(null);
        }
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using WEB_API.Models;
using WEB_APP.Models;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Azure;
using System.Net;
using System.Net.Http.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

namespace WEB_APP.Controllers
{
    public class ClaimController : Controller
    {
        private string apiUrl;

        public ClaimController()
        {
            var MyConfig = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            apiUrl = MyConfig.GetValue<string>("AppSettings:apiUrl")+"ClaimApi/";
        }

        public async Task<ActionResult> Index(string? Status = null, DateTime? DateFrom = null, DateTime? DateTo = null)
        {

            var currentAction = this.ControllerContext.RouteData.Values["action"].ToString();

            ClaimViewModel viewModel = new ClaimViewModel();
            viewModel.Status = Status;
            viewModel.DateFrom = DateFrom;
            viewModel.DateTo = DateTo;

            using (var client = new HttpClient()) {
                client.BaseAddress = new Uri(this.apiUrl);
                //HTTP GET
                var responseTask = client.GetAsync(String.Format(currentAction+ "?status={0}&DateFrom={1}&DateTo={2}", Status, DateFrom, DateTo));
                responseTask.Wait();
                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<IList<ClaimModel>>();
                    readTask.Wait();

                    viewModel.list_ = (List<ClaimModel>)readTask.Result;

                }
                else //web api sent error response 
                {
                    //log response status here...
                    ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                }

                // Fetch list of SelectListItems for statuses
                var claimStatusesUrl = "ClaimStatuses";
                if (Status != null)
                {
                    claimStatusesUrl += $"?Status={Uri.EscapeDataString(Status)}";
                }

                var claimStatusesResponse = await client.GetAsync(claimStatusesUrl);
                if (claimStatusesResponse.IsSuccessStatusCode)
                {
                    var statuses = await claimStatusesResponse.Content.ReadAsAsync<List<SelectListItem>>();
                    viewModel.Statuses = statuses;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to retrieve claim statuses from API.");
                }

                //responseTask = client.GetAsync(String.Format("ClaimStatuses?Status={0}", Status));
                //responseTask.Wait();
                //result = responseTask.Result;
                //if (result.IsSuccessStatusCode)
                //{
                //    var readTask = result.Content.ReadAsAsync<IList<SelectListItem>>();
                //    readTask.Wait();

                //    viewModel.Statuses = (List<SelectListItem>)readTask.Result;

                //}
                //else //web api sent error response 
                //{
                //    //log response status here...
                //    ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                //}
            }

            return View(viewModel);
        }


        [HttpGet]
        public IActionResult New()
        {
            EditClaimModel model = new EditClaimModel ();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.apiUrl);

                var responseTask = client.GetAsync(String.Format("ClaimStatuses?Status={0}", model.Status));
                responseTask.Wait();
                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<IList<SelectListItem>>();
                    readTask.Wait();

                    model.Statuses = (List<SelectListItem>)readTask.Result;

                }

                else //web api sent error response 
                {
                    //log response status here...
                    ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                }

            }

            return View(model); 
        }

        [HttpPost]
        public async Task<ActionResult> New(EditClaimModel model)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.apiUrl);
                //HTTP POST
                if (ModelState.IsValid)
                {
                    ClaimModel claimModel = new ClaimModel();
                    claimModel.PatientName = model.PatientName;
                    claimModel.DateOfService = model.DateOfService;
                    claimModel.MedicalProvider = model.MedicalProvider;
                    claimModel.Diagnosis = model.Diagnosis;
                    claimModel.ClaimAmount = model.ClaimAmount;

                    claimModel.Status = model.Status;
                    try
                    {
                        string json = JsonConvert.SerializeObject(claimModel);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await client.PostAsync("https://localhost:7017/api/ClaimApi/add", content);

                        if (response.IsSuccessStatusCode)
                        {
                            return Redirect("https://localhost:7298/Claim/Index");
                        }
                        else
                        {
                            // Handle unsuccessful response (optional)
                            ModelState.AddModelError(string.Empty, "Failed to update claim.");
                        }

                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, "Error: " + ex.Message);
                    }
                }

                //fetch statuses
                var responseTask = client.GetAsync(String.Format("ClaimStatuses?Status={0}", model.Status));
                responseTask.Wait();
                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<IList<SelectListItem>>();
                    readTask.Wait();

                    model.Statuses = (List<SelectListItem>)readTask.Result;

                }
                else //web api sent error response 
                {
                    //log response status here...
                    ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult View(int Id)
        {
            ClaimModel model = new ClaimModel();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.apiUrl);
                //HTTP GET
                var responseTask = client.GetAsync(String.Format("GetById?Id={0}", Id));
                responseTask.Wait();
                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<ClaimModel>();
                    readTask.Wait();
                    model = (ClaimModel)readTask.Result;

                }
                else //web api sent error response 
                {
                    //log response status here...
                    ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(int Id) {
            EditClaimModel model = new EditClaimModel();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.apiUrl);
                //HTTP GET
                var responseTask = client.GetAsync(String.Format("GetById?Id={0}", Id));
                responseTask.Wait();
                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<ClaimModel>();
                    readTask.Wait();
                    var claimModel = (ClaimModel)readTask.Result;

                    model.ClaimId = claimModel.ClaimId;
                    model.PatientName = claimModel.PatientName;
                    model.DateOfService = claimModel.DateOfService;
                    model.MedicalProvider = claimModel.MedicalProvider;
                    model.Diagnosis = claimModel.Diagnosis;
                    model.ClaimAmount = claimModel.ClaimAmount;
                    model.Status = claimModel.Status;
                }

                responseTask = client.GetAsync(String.Format("ClaimStatuses?Status={0}", model.Status));
                responseTask.Wait();
                result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<IList<SelectListItem>>();
                    readTask.Wait();

                    model.Statuses = (List<SelectListItem>)readTask.Result;

                }

                else //web api sent error response 
                {
                    //log response status here...
                    ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(EditClaimModel model)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.apiUrl);
                //HTTP POST
                if (ModelState.IsValid)
                {
                    ClaimModel claimModel = new ClaimModel();
                    claimModel.ClaimId = model.ClaimId;
                    claimModel.PatientName = model.PatientName;
                    claimModel.DateOfService = model.DateOfService;
                    claimModel.MedicalProvider = model.MedicalProvider;
                    claimModel.Diagnosis = model.Diagnosis;
                    claimModel.ClaimAmount = model.ClaimAmount;

                    claimModel.Status = model.Status;
                    var apiEndPoint = this.apiUrl + "update";
                    try
                    {
                        string json = JsonConvert.SerializeObject(claimModel);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await client.PostAsync("https://localhost:7017/api/ClaimApi/update", content);

                        if (response.IsSuccessStatusCode)
                        {
                            return Redirect("https://localhost:7298/Claim/Index");
                        }
                        else
                        {
                            // Handle unsuccessful response (optional)
                            ModelState.AddModelError(string.Empty, "Failed to update claim.");
                        }
                    
                    }
                    catch (Exception ex) {
                        ModelState.AddModelError(string.Empty, "Error: " + ex.Message);
                    }
                }

                //fetch statuses
                var responseTask = client.GetAsync(String.Format("ClaimStatuses?Status={0}", model.Status));
                responseTask.Wait();
                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<IList<SelectListItem>>();
                    readTask.Wait();

                    model.Statuses = (List<SelectListItem>)readTask.Result;

                }
                else //web api sent error response 
                {
                    //log response status here...
                    ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int ClaimId) {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.apiUrl);
                //HTTP POST
                try
                {
                    // Construct the URL with the parameters
                    string apiUrl = $"https://localhost:7017/api/ClaimApi/Delete?ClaimId={ClaimId}";

                    // Send the POST request (assuming empty content for this example)
                    HttpResponseMessage response = await client.PostAsync(apiUrl, null);

                    if (response.IsSuccessStatusCode)
                    {
                        return Redirect("https://localhost:7298/Claim/Index");
                    }
                    else 
                    {
                        // Store ModelState errors in TempData for redirection
                        TempData["ModelStateErrors"] = ModelState;

                        // Redirect to the index page with error message
                        return RedirectToAction("Index", "Claim");
                    }

                }
                catch (Exception ex)
                {
                    // Store ModelState errors in TempData for redirection
                    TempData["ModelStateErrors"] = ModelState;

                    // Redirect to the index page with error message
                    return RedirectToAction("Index", "Claim");
                }
            }
        }


    }
}

using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using WEB_API.Models;

namespace WEB_APP.Models
{
    public class ClaimViewModel {
        public string? Status { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public List<ClaimModel> list_ { get; set; }
        public List<SelectListItem> Statuses { get; set; }
    }

    public class SearchModel
    {
        public string? Status { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}

using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BusTracking.Web.Areas.Parent.Controllers
{
    public class TrackingController : ParentBaseController
    {
        private readonly ITripService _trip;
        public TrackingController(ITripService t) => _trip = t;

        public async Task<IActionResult> Track(int tripId)
        {
            var students = await _trip.GetTripStudentsAsync(tripId);
            var location = await _trip.GetLatestLocationAsync(tripId);
            ViewBag.TripId = tripId;
            ViewBag.Location = location.Data;
            return View(students.Data);
        }
    }
}

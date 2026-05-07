using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusTracking.Web.Controllers
{
    [Authorize(Roles = "Student,Parent")]
    public class TrackingController : BaseController
    {
        private readonly ITripService _trip;
        public TrackingController(ITripService trip) => _trip = trip;

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

namespace BusTracking.Common.Models
{
    public class GpsCoordinate
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        /// <summary>Haversine distance in kilometres</summary>
        public static double DistanceKm(GpsCoordinate a, GpsCoordinate b)
        {
            const double R = 6371;
            var dLat = ToRad((double)(b.Latitude - a.Latitude));
            var dLon = ToRad((double)(b.Longitude - a.Longitude));
            var sin2Lat = Math.Sin(dLat / 2) * Math.Sin(dLat / 2);
            var sin2Lon = Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = Math.Acos(1 - 2 * (sin2Lat + Math.Cos(ToRad((double)a.Latitude))
                                                * Math.Cos(ToRad((double)b.Latitude))
                                                * sin2Lon));
            return R * c;
        }

        private static double ToRad(double deg) => deg * Math.PI / 180;
    }
}

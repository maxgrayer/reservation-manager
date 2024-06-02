using System;
using System.Collections.Generic;
using System.Linq;

namespace ReservationManager
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Initialize the manager
            var manager = new ReservationManager();

            // Create sample reservations for the time slot (using DateTime.Now versus pre-defined slots)
            var reservationA = new Reservation(20, DateTime.Now, TimeSpan.FromMinutes(120));
            var reservationB = new Reservation(20, DateTime.Now, TimeSpan.FromMinutes(120));
            var reservationC = new Reservation(20, DateTime.Now, TimeSpan.FromMinutes(120));

            // Try to make the reservations
            bool success = manager.TryAddReservation(reservationA);
            Console.WriteLine($"ReservationA success: {success}"); // TRUE
            success = manager.TryAddReservation(reservationB);
            Console.WriteLine($"ReservationB success: {success}"); // TRUE
            success = manager.TryAddReservation(reservationC);
            Console.WriteLine($"ReservationC success: {success}"); // FALSE
        }
    }

    public class Reservation
    {
        public int Quantity { get; private set; }
        public DateTime StartTime { get; private set; }
        public TimeSpan Duration { get; private set; }

        public Reservation(int quantity, DateTime startTime, TimeSpan duration)
        {
            Quantity = quantity;
            StartTime = startTime;
            Duration = duration;
        }
    }

    /// <summary>
    /// Class responsible for managing reservations:
    /// * maintains capacity limits (50 per-slot, 450 overall)
    /// * checks if reservation is valid, adds if valid
    /// ** reservation is valid if slot is not exceeded and overall isn't exceeded during duration
    /// </summary>
    public class ReservationManager
    {
        private const int TimeSlotCapacity = 50;
        private const int VenueCapacity = 450;
        private static readonly TimeSpan TimeSlotInterval = TimeSpan.FromMinutes(15);

        private List<Reservation> reservations = new List<Reservation>();

        /// <summary>
        /// Attempt to make a reservation
        /// </summary>
        /// <param name="newReservation">The reservation to check</param>
        /// <returns>true if reservation was successfully added, false otherwise</returns>
        public bool TryAddReservation(Reservation newReservation)
        {
            if (IsValidReservation(newReservation))
            {
                reservations.Add(newReservation);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks for slot capacity and venue capacity to determine if reservation is valid
        /// </summary>
        /// <param name="newReservation">The reservation to check</param>
        /// <returns>true if reservation does not exceed capacities, false otherwise</returns>
        private bool IsValidReservation(Reservation newReservation)
        {
            DateTime endTime = newReservation.StartTime + newReservation.Duration;
            for (DateTime timeSlot = newReservation.StartTime; timeSlot < endTime; timeSlot += TimeSlotInterval)
            {
                int slotCapacity = GetSlotCapacity(timeSlot);
                int venueCapacity = GetVenueCapacityForSlot(timeSlot, timeSlot + TimeSlotInterval);

                if (slotCapacity + newReservation.Quantity > TimeSlotCapacity ||
                    venueCapacity + newReservation.Quantity > VenueCapacity)
                {
                    return false;
                }
            }
            return true;
        }

        private int GetSlotCapacity(DateTime timeSlot)
        {
            // using LINQ to gather quantities per 15-min slot within the range
            // could be simplified with per-defined time slots
            return reservations
                .Where(r => r.StartTime <= timeSlot && timeSlot < r.StartTime + r.Duration)
                .Sum(r => r.Quantity);
        }

        private int GetVenueCapacityForSlot(DateTime slotStart, DateTime slotEnd)
        {
            return reservations
                .Where(r => r.StartTime < slotEnd && r.StartTime + r.Duration > slotStart)
                .Sum(r => r.Quantity);
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GatewayLab2.Models;
using GatewayLab2.Managers;
using GatewayLab2.Views;
using Microsoft.AspNetCore.Authorization;

namespace GatewayLab2.Controllers
{
    [Route("api/v1/")]
    [ApiController]
    public class GatewayController : ControllerBase
    {
        string reservationService { get; }
        ReservationManager ReservationManager { get; }

        string paymentService { get; }
        PaymentManager PaymentManager { get; }

        string loyaltyService { get; }
        LoyaltyManager LoyaltyManager { get; }

        QueueManager queueManager { get; }

        public GatewayController()
        {
            reservationService = Environment.GetEnvironmentVariable("RESERVATION") ?? "localhost:8070";
            ReservationManager = new ReservationManager(reservationService);

            paymentService = Environment.GetEnvironmentVariable("PAYMENT") ?? "localhost:8060";
            PaymentManager = new PaymentManager(paymentService);

            loyaltyService = Environment.GetEnvironmentVariable("LOYALTY") ?? "localhost:8050";
            LoyaltyManager = new LoyaltyManager(loyaltyService);

            queueManager = new QueueManager();
            queueManager.StartQueue();
        }

        [HttpGet("hotels")]
        [Authorize("read:data")]
        public ActionResult<IEnumerable<Hotel>> Hotels(int page, int size)
        {
            var hotels = this.ReservationManager.GetHotels();
            if(hotels == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            PaginationResponse<Hotel> hotelsResponse = new PaginationResponse<Hotel>();
            hotelsResponse.Page = page;
            hotelsResponse.Size = size;
            hotelsResponse.Items = hotels.ToArray();
            hotelsResponse.TotalElements = hotels.Count();
            return Ok(hotelsResponse);
        }

        [HttpGet("me")]
        [Authorize("read:data")]
        public ActionResult<UserInfoResponse<object>> GetUserInfo()
        {
            string userInfoURL = User.Claims.FirstOrDefault(claim => claim.Type == "aud" && claim.Value.EndsWith("userinfo"))?.Value;
            string authorization = this.Request.Headers["Authorization"];
            string username = UserManager.GetUser(userInfoURL, authorization).Nickname;
            var loyalty = LoyaltyManager.GetLoyalties()
                                        ?.FirstOrDefault(loyalty => loyalty.Username == username);

            var reservations = ReservationManager.GetReservations();
            if(reservations == null)
            {
                return StatusCode(500);
            }
            reservations = reservations.Where(reser => reser.Username == username);

            var hotels = ReservationManager.GetHotels();
            if(hotels == null)
            {
                return StatusCode(500);
            }

            var payments = PaymentManager.GetPayments();

            List<ReservationResponse> responseList = new List<ReservationResponse>();
            foreach (var reservation in reservations)
            {
                var hotel = hotels.FirstOrDefault(h => h.ID == reservation.Hotel_ID);
                var payment = payments?.FirstOrDefault(p => p.Payment_UID == reservation.Payment_UID);

                ReservationResponse response = new ReservationResponse();
                response.reservationUid = reservation.Reservation_UID;
                response.Hotel = (HotelView)hotel;
                response.startDate = reservation.Start_Date.ToString("yyyy-MM-dd");
                response.endDate = reservation.End_Date.ToString("yyyy-MM-dd");
                response.status = reservation.Status;
                response.Payment = (payment != null ? ((PaymentView)payment) : null);

                responseList.Add(response);
            }

            if (loyalty != null)
            {
                return Ok(new UserInfoResponse<LoyaltyInfoResponse>() { Loyalty = (loyalty != null ? (LoyaltyInfoResponse)loyalty : null), Reservations = responseList });
            }

            return Ok(new UserInfoResponse<string>() { Loyalty = "", Reservations = responseList });

            //return BadRequest();
        }

        [HttpGet("reservations")]
        [Authorize("read:data")]
        public ActionResult<IEnumerable<ReservationResponse>> GetReservations()
        {
            string userInfoURL = User.Claims.FirstOrDefault(claim => claim.Type == "aud" && claim.Value.EndsWith("userinfo"))?.Value;
            string authorization = this.Request.Headers["Authorization"];
            string username = UserManager.GetUser(userInfoURL, authorization).Nickname;
            var reservations = ReservationManager.GetReservations();
            if(reservations == null)
            {
                return StatusCode(500);
            }
            
            reservations = reservations.Where(reser => reser.Username == username);

            var hotels = ReservationManager.GetHotels();
            if (hotels == null)
            {
                return StatusCode(500);
            }

            var payments = PaymentManager.GetPayments();

            List<ReservationResponse> responseList = new List<ReservationResponse>();
            foreach (var reservation in reservations)
            {
                var hotel = hotels.FirstOrDefault(h => h.ID == reservation.Hotel_ID);
                var payment = payments?.FirstOrDefault(p => p.Payment_UID == reservation.Payment_UID);

                ReservationResponse response = new ReservationResponse();
                response.reservationUid = reservation.Reservation_UID;
                response.Hotel = (HotelView)hotel;
                response.startDate = reservation.Start_Date.ToString("yyyy-MM-dd");
                response.endDate = reservation.End_Date.ToString("yyyy-MM-dd");
                response.status = reservation.Status;
                response.Payment = (payment != null ? ((PaymentView)payment) : null);

                responseList.Add(response);
            }

            if (responseList.Count > 0)
            {
                return Ok(responseList);
            }
            
            return Ok();
        }

        [HttpGet("reservations/{reservationUid}")]
        [Authorize("read:data")]
        public ActionResult<ReservationResponse> GetReservation(Guid reservationUid)
        {
            string userInfoURL = User.Claims.FirstOrDefault(claim => claim.Type == "aud" && claim.Value.EndsWith("userinfo"))?.Value;
            string authorization = this.Request.Headers["Authorization"];
            string username = UserManager.GetUser(userInfoURL, authorization).Nickname;
            var reservations = ReservationManager.GetReservations();

            if(reservations == null)
            {
                return StatusCode(500);
            }
            
            var reservation = reservations.FirstOrDefault(res => res.Reservation_UID == reservationUid &&
                                                                       res.Username == username);
            var hotels = ReservationManager.GetHotels();
            if(hotels == null)
            {
                return StatusCode(500);
            }

            var hotel = hotels.FirstOrDefault(hotel => hotel.ID == reservation?.Hotel_ID);

            var payment = PaymentManager.GetPayments()
                                          ?.FirstOrDefault(payment => payment.Payment_UID == reservation?.Payment_UID);
            if (reservation != null && hotel != null)
            {
                ReservationResponse response = new ReservationResponse();
                response.reservationUid = reservation.Reservation_UID;
                response.Hotel = (HotelView)hotel;
                response.startDate = reservation.Start_Date.ToString("yyyy-MM-dd");
                response.endDate = reservation.End_Date.ToString("yyyy-MM-dd");
                response.status = reservation.Status;
                response.Payment = (payment != null ? ((PaymentView)payment) : null);

                return Ok(response);
            }

            return NotFound();
        }

        [HttpPost("reservations")]
        [Authorize("write:data")]
        public ActionResult BookHotel(BookHotelView view)
        {
            string userInfoURL = User.Claims.FirstOrDefault(claim => claim.Type == "aud" && claim.Value.EndsWith("userinfo"))?.Value;
            string authorization = this.Request.Headers["Authorization"];
            string username = UserManager.GetUser(userInfoURL, authorization).Nickname;
            if (string.IsNullOrEmpty(username))
            {
                return BadRequest();
            }

            var hotels = ReservationManager.GetHotels();
            if(hotels == null)
            {
                return StatusCode(503);
            }
                
            var hotel = hotels.FirstOrDefault(hotel => hotel.HotelUID == view.hotelUid);
            if (hotel == null)
            {
                return BadRequest("No such hotel");
            }

            var loyalties = LoyaltyManager.GetLoyalties();
            if(loyalties == null)
            {
                return StatusCode(503, new Message() { message = "Loyalty Service unavailable" } );
            }

            var loyalty = loyalties.FirstOrDefault(loyalty => loyalty.Username == username);
            if(loyalty == null)
            {
                return BadRequest();
            }


            double clientCost = PaymentManager.GetReservationCost(hotel.Price, view.start, view.end);
            int discount = LoyaltyManager.GetDiscount(loyalty);
            clientCost = clientCost - (clientCost / 100 * discount);

            var payment = PaymentManager.PayReservation("PAID", clientCost);
            if(payment == null)
            {
                return StatusCode(503);
            }

            loyalty.Reservation_Count++;
            if(loyalty.Reservation_Count >= 10)
            {
                loyalty.Status = "SILVER";
            }
            if (loyalty.Reservation_Count >= 20)
            {
                loyalty.Status = "GOLD";
            }
            LoyaltyManager.UpdateLoyalty(loyalty);

            var reservation = ReservationManager.BookHotel(username, hotel, payment, view.start, view.end);

            CreateReservationResponse response = new CreateReservationResponse();
            response.reservationUid = reservation.Reservation_UID;
            response.hotelUid = hotel.HotelUID;
            response.startDate = view.start.ToString("yyyy-MM-dd");
            response.endDate = view.end.ToString("yyyy-MM-dd");
            response.discount = loyalty.Discount;
            response.status = reservation.Status;
            response.Payment = new PaymentView() { price = (int)payment.Price, status = payment.Status };
            return Ok(response);
        }

        [HttpDelete("reservations/{reservationUid}")]
        [Authorize("write:data")]
        public ActionResult CancelReservation(Guid reservationUid)
        {
            var reservations = ReservationManager.GetReservations();
            if(reservations == null)
            {
                return StatusCode(500);
            }
            var reservation = reservations.FirstOrDefault(res => res.Reservation_UID == reservationUid);

            if(reservation == null)
            {
                return NotFound("no such reservation");
            }

            var cancelPaymentRes = PaymentManager.CancelPayment(reservation.Payment_UID);
            if(cancelPaymentRes.Success == false)
            {
                return StatusCode(500);
            }

            var loyaltyRes = LoyaltyManager.DecrementLoyalty(reservation.Username);
            if(loyaltyRes.Success == false)
            {
                queueManager.AddNewAction(() =>
                {
                    var resDecr = LoyaltyManager.DecrementLoyalty(reservation.Username);
                    return resDecr.Success;
                });
            }

            if (ReservationManager.CancelReservation(reservationUid).Success)
            {
                return NoContent();
            }

            return NotFound();
        }

        [HttpGet("loyalty")]
        [Authorize("read:data")]
        public ActionResult<Loyalty> GetLoyalty()
        {
            string userInfoURL = User.Claims.FirstOrDefault(claim => claim.Type == "aud" && claim.Value.EndsWith("userinfo"))?.Value;
            string authorization = this.Request.Headers["Authorization"];
            string username = UserManager.GetUser(userInfoURL, authorization).Nickname;
            var loyalties = LoyaltyManager.GetLoyalties();
            if(loyalties == null)
            {
                return StatusCode(503, new Message() { message = "Loyalty Service unavailable" } );
            }
            var loyalty = loyalties.FirstOrDefault(loyalty => loyalty.Username == username);

            if (loyalty != null)
            {
                return Ok((LoyaltyInfoResponse)loyalty);
            }

            return BadRequest();
        }
    }
}
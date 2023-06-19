using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using DankWaifu.Net;
using Newtonsoft.Json;
using TextNow.Waifu.Json;

namespace TextNow.Waifu
{
    public class TextNowClientLoggedIn : TextNowClientBase
    {
        private readonly TextNowSession _session;

        public TextNowClientLoggedIn(
            HttpWaifu client, TextNowSession session) : base(client, session.Account.Device)
        {
            _session = session;
        }

        public async Task<TextNowPhoneNumberReservationResponse> CreatePhoneNumberReservation(
            string areaCode)
        {
            const HttpMethod httpMethod = HttpMethod.POST;
            const string endpoint = "phone_numbers/reserve";

            var queryParams = new Dictionary<string, string>
            {
                ["client_type"] = "TN_ANDROID",
                ["client_id"] = _session.ClientId
            };

            var postParams = new Dictionary<string, string>
            {
                ["json"] = $"{{\"reservation_length\":\"70\",\"area_code\":\"{areaCode}\"}}"
            };

            var apiReq = new TextNowApiRequest(httpMethod, endpoint)
            {
                QueryParams = queryParams,
                PostParams = postParams
            };

            var response = await SendApiRequest(apiReq)
                .ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.OK)
                throw CreateInvalidStatusCodeEx(endpoint, response.StatusCode);

            var json = await Task.Run(
                () => JsonConvert.DeserializeObject<TextNowPhoneNumberReservationResponse>(
                    response.ContentBody
                )
            ).ConfigureAwait(false);

            if (json.Result == null)
                throw CreateInvalidJsonResultEx(endpoint, response.StatusCode);

            if (json.ErrorCode != null)
                throw CreateInvalidErrorCodeEx(endpoint, json.ErrorCode, response.StatusCode);

            if (json.Result.PhoneNumbers == null ||
                json.Result.PhoneNumbers.Count == 0)
            {
                throw CreateTextNowApiEx(
                    endpoint,
                    $"did not return any phone numbers with area code {areaCode}",
                    response.StatusCode
                );
            }

            if (string.IsNullOrWhiteSpace(json.Result.ReservationId))
            {
                throw CreateTextNowApiEx(
                    endpoint,
                    "did not return a reservation ID",
                    response.StatusCode);
            }

            return json;
        }

        public async Task<TextNowAssignReservedResponse> CreatePhoneNumberAssignment(
            string reservationId,
            string phoneNumber)
        {
            const HttpMethod httpMethod = HttpMethod.POST;
            const string endpoint = "phone_numbers/assign_reserved";

            var queryParams = new Dictionary<string, string>
            {
                ["client_type"] = "TN_ANDROID",
                ["client_id"] = _session.ClientId
            };

            var postParams = new Dictionary<string, string>
            {
                ["json"] = $"{{\"reservation_id\":\"{reservationId}\",\"phone_number\":\"{phoneNumber}\"}}"
            };

            var apiReq = new TextNowApiRequest(httpMethod, endpoint)
            {
                QueryParams = queryParams,
                PostParams = postParams
            };

            var response = await SendApiRequest(apiReq)
                .ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.OK)
                throw CreateInvalidStatusCodeEx(endpoint, response.StatusCode);

            var json = await Task.Run(
                () => JsonConvert.DeserializeObject<TextNowAssignReservedResponse>(
                    response.ContentBody
                )
            ).ConfigureAwait(false);

            if (json.ErrorCode != null)
                throw CreateInvalidErrorCodeEx(endpoint, json.ErrorCode, response.StatusCode);

            if (json.Result == null)
                throw CreateInvalidJsonResultEx(endpoint, response.StatusCode);

            if (string.IsNullOrWhiteSpace(json.Result.PhoneNumber))
            {
                throw CreateTextNowApiEx(
                    endpoint,
                    "did not return a phone number",
                    response.StatusCode
                );
            }

            return json;
        }

        public async Task DeletePhoneNumberReservation(string reservationId)
        {
            const HttpMethod httpMethod = HttpMethod.DELETE;
            const string endpoint = "phone_numbers/reserve";

            var queryParams = new Dictionary<string, string>
            {
                ["client_type"] = "TN_ANDROID",
                ["client_id"] = _session.ClientId
            };

            var postParams = new Dictionary<string, string>
            {
                ["json"] = $"{{\"reservation_id\":\"{reservationId}\"}}"
            };

            var apiReq = new TextNowApiRequest(httpMethod, endpoint)
            {
                QueryParams = queryParams,
                PostParams = postParams
            };

            var response = await SendApiRequest(apiReq)
                .ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.OK)
                throw CreateInvalidStatusCodeEx(endpoint, response.StatusCode);
        }

        public async Task<TextNowUserInfoResponse> RetrieveUserInfo()
        {
            const HttpMethod httpMethod = HttpMethod.GET;
            var endpoint = $"users/{_session.Account.Username}";

            var queryParams = new Dictionary<string, string>
            {
                ["client_type"] = "TN_ANDROID",
                ["client_id"] = _session.ClientId
            };

            var apiReq = new TextNowApiRequest(httpMethod, endpoint)
            {
                QueryParams = queryParams
            };

            var response = await SendApiRequest(apiReq)
                .ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.OK)
                throw CreateInvalidStatusCodeEx(endpoint, response.StatusCode);

            var json = await Task.Run(
                () => JsonConvert.DeserializeObject<TextNowUserInfoResponse>(
                    response.ContentBody
                )
            ).ConfigureAwait(false);

            return json;
        }

        // TODO:
        public async Task<TextNowMessagesResponse> RetrieveMessages()
        {
            const HttpMethod httpMethod = HttpMethod.GET;
            var endpoint = $"users/{_session.Account.Username}/messages";

            var queryParams = new Dictionary<string, string>
            {
                ["client_type"] = "TN_ANDROID",
                ["client_id"] = _session.ClientId,
                ["start_message_id"] = "1",
                ["page_size"] = "30",
                ["direction"] = "future",
                ["get_all"] = "1"
            };

            var apiReq = new TextNowApiRequest(httpMethod, endpoint)
            {
                QueryParams = queryParams
            };

            var response = await SendApiRequest(apiReq)
                .ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.OK)
                throw CreateInvalidStatusCodeEx(endpoint, response.StatusCode);

            var json = await Task.Run(
                () => JsonConvert.DeserializeObject<TextNowMessagesResponse>(
                    response.ContentBody
                )
            ).ConfigureAwait(false);

            return json;
        }

        public async Task DeleteMessage(string withPhoneNumber)
        {
            if (withPhoneNumber == null)
                throw new ArgumentNullException(nameof(withPhoneNumber));

            if (string.IsNullOrWhiteSpace(withPhoneNumber))
                throw new ArgumentException($"{nameof(withPhoneNumber)} must not be an empty string");

            const HttpMethod httpMethod = HttpMethod.DELETE;
            var endpoint = $"users/{_session.Account.Username}/conversations/{HttpHelpers.UrlEncode(withPhoneNumber)}";

            var queryParams = new Dictionary<string, string>
            {
                ["client_type"] = "TN_ANDROID",
                ["client_id"] = _session.ClientId
            };

            var apiReq = new TextNowApiRequest(httpMethod, endpoint)
            {
                QueryParams = queryParams
            };

            var response = await SendApiRequest(apiReq)
                .ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.OK)
                throw CreateInvalidStatusCodeEx(endpoint, response.StatusCode);
        }
    }
}
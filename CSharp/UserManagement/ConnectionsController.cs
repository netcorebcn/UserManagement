using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ploeh.Samples.UserManagement
{
    public class ConnectionsController : ApiController
    {
        public ConnectionsController(
            IUserCache userCache,
            IUserRepository userRepository)
        {
            UserCache = userCache;
            UserRepository = userRepository;
        }

        public IUserCache UserCache { get; }

        public IUserRepository UserRepository { get; }

        public IHttpActionResult Post(string userId, string otherUserId) =>
            LookupUser(userId) switch
            {
                FoundUserLookupResult foundResult => LookupOtherUser(foundResult.User, otherUserId),
                NotFoundUserLookupResult _ => BadRequest("User not found."),
                InvalidIdUserLookupResult _ => BadRequest("Invalid user ID."),
                _ => BadRequest("Sorry no sum types in C#")
            };

        private IHttpActionResult LookupOtherUser(User user, string otherUserId) =>
            LookupUser(otherUserId) switch
            {
                FoundUserLookupResult otherFoundResult => ConnectUsers(user, otherFoundResult.User),
                NotFoundUserLookupResult _ => BadRequest("Other user not found."),
                InvalidIdUserLookupResult _ => BadRequest("Invalid ID for other user."),
                _ => BadRequest("Sorry no sum types in C#")
            };

        private IHttpActionResult ConnectUsers(User user, User otherUser)
        {
            user.Connect(otherUser);
            UserRepository.Update(user);
            return Ok(otherUser);
        }

        public interface IUserLookupResult { }

        public class FoundUserLookupResult : IUserLookupResult
        {
            public FoundUserLookupResult(User user)
            {
                if (user == null)
                    throw new ArgumentNullException(nameof(user));
                User = user;
            }

            public User User { get; }
        }

        public class InvalidIdUserLookupResult : IUserLookupResult { }

        public class NotFoundUserLookupResult : IUserLookupResult { }

        private IUserLookupResult LookupUser(string id)
        {
            var user = UserCache.Find(id);
            if (user != null)
                return new FoundUserLookupResult(user);

            int userInt;
            if (!int.TryParse(id, out userInt))
                return new InvalidIdUserLookupResult();

            user = UserRepository.ReadUser(userInt);
            if (user == null)
                return new NotFoundUserLookupResult();

            return new FoundUserLookupResult(user);
        }
    }
}
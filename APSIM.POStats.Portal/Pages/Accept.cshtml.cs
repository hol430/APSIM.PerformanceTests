using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace APSIM.POStats.Portal.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class AcceptModel : PageModel
    {
        /// <summary>Constructor</summary>
        public AcceptModel()
        {
        }

        /// <summary>The pull request id.</summary>
        public int PullRequestId { get; private set; }

        /// <summary>Invoked when page is first loaded.</summary>
        /// <param name="id">The id of the pull request to work with.</param>
        public void OnGet(int id)
        {
            PullRequestId = id;
        }

        /// <summary>Invoked when user clicks submit.</summary>
        public void OnPost()
        {
            var pwd = Request.Form["Password"].ToString();
            //ValidatePassword(pwd);
        }
    }
}

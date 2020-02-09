using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnetcore.urlshortener.contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace UrlShortenerApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IUrlShortenerService _urlShortenerService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            IUrlShortenerService urlShortenerService,
            ILogger<IndexModel> logger)
        {
            _urlShortenerService = urlShortenerService;
            _logger = logger;
        }
        [BindProperty]
        public string LongUrl { get; set; }
        [BindProperty]
        public string ShortUrl { get; set; }

        public void OnGet()
        {

        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            bool isUri = Uri.IsWellFormedUriString(LongUrl, UriKind.Absolute);
            if (!isUri)
            {
                return Page();
            }

            var shortUrl = new ShortUrl
            {
                LongUrl = LongUrl,
                Expiration = DateTime.UtcNow.AddMinutes(2)
            };

            shortUrl = await _urlShortenerService.UpsertShortUrlAsync("1",shortUrl);

            this.ShortUrl = $"{Request.Scheme}://{Request.Host}/s/{shortUrl.Id}";

            return Page();
        }
    }
}

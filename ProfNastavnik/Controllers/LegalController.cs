using System;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using ProfNastavnik.Security;

namespace ProfNastavnik.Controllers
{
    [Route("api/[controller]")]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class LegalController : ControllerBase
    {
        // GET api/v1/Legal/List
        [HttpGet("List")]
        public ActionResult List()
        {
            return Ok(new
            {
                version = LegalTexts.Version,
                docs = LegalTexts.Docs.Select(x => new { key = x.Key, title = x.Value.Title }).ToList()
            });
        }

        [HttpGet("{key}")]
        public ActionResult Get(string key)
        {
            if (!LegalTexts.Docs.TryGetValue(key ?? "", out var doc)) return NotFound("Документ не найден");
            return Ok(new { key, title = doc.Title, version = LegalTexts.Version, text = doc.Text });
        }
    }

    /// <summary>
    /// Веб-страницы для магазинов приложений: /legal/privacy, /legal/terms, /legal/offer, /legal/consent
    /// и /legal/delete-account (ссылка для «Удаление аккаунта» в Google Play Console и политика конфиденциальности).
    /// </summary>
    [Route("legal")]
    [ApiController]
    public class LegalPagesController : ControllerBase
    {
        private static string Page(string title, string bodyHtml) =>
            "<!DOCTYPE html><html lang=\"ru\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">" +
            "<title>" + WebUtility.HtmlEncode(title) + " — Воркшип</title>" +
            "<style>body{font-family:system-ui,Segoe UI,Arial,sans-serif;max-width:760px;margin:0 auto;padding:20px;line-height:1.55;color:#222}" +
            "h1{font-size:24px}nav a{margin-right:14px}input,button{font-size:16px;padding:10px;margin:6px 0;width:100%;box-sizing:border-box}" +
            "button{background:#b71c1c;color:#fff;border:0;border-radius:8px;cursor:pointer}.ok{color:#1b7f3b}.err{color:#b71c1c}</style></head><body>" +
            "<nav><a href=\"/legal/privacy\">Политика конфиденциальности</a><a href=\"/legal/terms\">Соглашение</a><a href=\"/legal/offer\">Оферта</a>" +
            "<a href=\"/legal/consent\">Согласие на обработку данных</a><a href=\"/legal/delete-account\">Удалить аккаунт</a></nav>" +
            bodyHtml + "</body></html>";

        [HttpGet("{key}")]
        public ContentResult Doc(string key)
        {
            if (key == "delete-account") return DeleteAccountPage();
            if (!LegalTexts.Docs.TryGetValue(key, out var doc))
                return Content(Page("Не найдено", "<h1>Документ не найден</h1>"), "text/html; charset=utf-8", Encoding.UTF8);
            string text = WebUtility.HtmlEncode(doc.Text).Replace("\r\n", "\n").Replace("\n", "<br>");
            return Content(Page(doc.Title, "<h1>" + WebUtility.HtmlEncode(doc.Title) + "</h1><p>" + text + "</p>"), "text/html; charset=utf-8", Encoding.UTF8);
        }

        private ContentResult DeleteAccountPage()
        {
            string html =
                "<h1>Удаление аккаунта «Воркшип»</h1>" +
                "<p>Здесь можно удалить аккаунт и связанные с ним данные без установки приложения. Удаляются профиль, отклики, переписка, отзывы и обращения. " +
                "Платёжные записи сохраняются без привязки к вашему профилю на срок, установленный законом. Действие необратимо.</p>" +
                "<p>Также удалить аккаунт можно в приложении: «Настройки» → «Удалить аккаунт».</p>" +
                "<input id=\"email\" type=\"email\" placeholder=\"Email аккаунта\" autocomplete=\"username\">" +
                "<input id=\"password\" type=\"password\" placeholder=\"Пароль\" autocomplete=\"current-password\">" +
                "<label><input id=\"sure\" type=\"checkbox\" style=\"width:auto\"> Я понимаю, что данные будут удалены безвозвратно</label>" +
                "<button id=\"go\">Удалить аккаунт</button><p id=\"msg\"></p>" +
                "<script>document.getElementById('go').onclick=async function(){var m=document.getElementById('msg');m.className='';" +
                "if(!document.getElementById('sure').checked){m.className='err';m.textContent='Подтвердите, что понимаете последствия';return;}" +
                "try{var r=await fetch('/api/v1/Account/DeleteByCredentials',{method:'POST',headers:{'Content-Type':'application/json'}," +
                "body:JSON.stringify({email:document.getElementById('email').value,password:document.getElementById('password').value})});" +
                "var t=await r.text();if(r.ok){m.className='ok';m.textContent='Аккаунт и данные удалены.';}else{m.className='err';" +
                "try{t=JSON.parse(t);}catch(e){}m.textContent=(typeof t==='string'&&t)?t:'Не удалось удалить аккаунт';}}" +
                "catch(e){m.className='err';m.textContent='Нет соединения с сервером';}};</script>";
            return Content(Page("Удаление аккаунта", html), "text/html; charset=utf-8", Encoding.UTF8);
        }
    }

    /// <summary>Версия приложения: GET api/v1/AppConfig/Info (публично).</summary>
    [Route("api/[controller]")]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AppConfigController : ControllerBase
    {
        [HttpGet("Info")]
        public ActionResult Info()
        {
            return Ok(new
            {
                minVersionCode = AppRuntime.MinVersionCode,
                latestVersionCode = AppRuntime.LatestVersionCode,
                storeUrl = AppRuntime.StoreUrl,
                legalVersion = LegalTexts.Version
            });
        }
    }
}

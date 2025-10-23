using Ganss.Xss;

namespace Atos.Application.Services;

public class HtmlSanitizerService
{
  private readonly HtmlSanitizer _sanitizer;

  public HtmlSanitizerService()
  {
    _sanitizer = new HtmlSanitizer();

    _sanitizer.AllowedTags.UnionWith(new[]
    {
      "a", "abbr", "b", "blockquote", "br", "code", "div", "em", "h1", "h2", "h3", "h4", "h5", "h6",
      "hr", "i", "img", "li", "ol", "p", "pre", "section", "small", "span", "strong", "sub", "sup", "table",
      "tbody", "td", "th", "thead", "tr", "u", "ul"
    });

    _sanitizer.AllowedAttributes.UnionWith(new[]
    {
      "href", "title", "target", "rel", "class", "style", "id", "name", "src", "alt", "data-*", "colspan", "rowspan"
    });

    _sanitizer.AllowedSchemes.UnionWith(new[] { "http", "https", "mailto", "data" });
    _sanitizer.AllowDataAttributes = true;
  }

  public string Sanitize(string? html) =>
    string.IsNullOrWhiteSpace(html) ? string.Empty : _sanitizer.Sanitize(html);
}

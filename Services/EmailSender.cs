using EcobayApp.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using System.Net.Mail;
using System.Threading.Tasks.Dataflow;

public interface IEmailSender
{
	Task SendRequestCreatedAsync(Client client, Request request, City? city, PageCity? pageCity);
}

public class EmailSender : IEmailSender
{
	private readonly EmailSettings _settings;

	public EmailSender(IOptions<EmailSettings> options)
	{
		_settings = options.Value;
	}

	public async Task SendRequestCreatedAsync(Client client, Request request, City? city, PageCity? pageCity)
	{
		var email = new MimeMessage();
		email.From.Add(MailboxAddress.Parse(_settings.From));
		email.To.Add(MailboxAddress.Parse(_settings.To));
		email.Subject = $"Новая заявка Ecobay: {client.Phone}";

		var cityName = city?.Name ?? pageCity?.City?.Name ?? "Не указан";
		var source = request.Source ?? "-";

		var html = $@"
			<h2>Новая заявка Ecobay.by</h2>
			<p><strong>Дата:</strong> {request.DateCreate:dd.MM.yyyy HH:mm}</p>
			<p><strong>Имя:</strong> {client.Name}</p>
			<p><strong>Телефон:</strong> <a href=""tel:{client.Phone}"">{client.Phone}</a></p>
			<p><strong>Город:</strong> {cityName}</p>
			<p><strong>Сообщение:</strong> {request.Message}</p>
			<p><strong>Источник:</strong> {source}</p>
			<br/>
			<strong>Сервис представлен ArtBavial Software</strong>";


		email.Body = new TextPart(TextFormat.Html) { Text = html };

		using var smtp = new MailKit.Net.Smtp.SmtpClient();
		await smtp.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls);
		await smtp.AuthenticateAsync(_settings.SmtpUser, _settings.SmtpPass);
		await smtp.SendAsync(email);
		await smtp.DisconnectAsync(true);
	}
}

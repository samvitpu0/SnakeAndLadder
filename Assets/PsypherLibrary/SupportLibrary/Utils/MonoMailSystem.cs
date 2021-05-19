using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using PsypherLibrary.SupportLibrary.Extensions;
using PsypherLibrary.SupportLibrary.Utils.Generics;
using UnityEngine;

namespace PsypherLibrary.SupportLibrary.Utils
{
    public class MonoMailSystem : GenericSingleton<MonoMailSystem>
    {
        const string MAIL_USERNAME = "misc.mystiq";
        const string MAIL_HOST = "gmail.com";
        const string MAIL_PASSKEY = "misc654321";
        public const string MAIL_CS = "blackbambooz.india@gmail.com";

        private SmtpClient _smtpServer;
        private string _mailAddress;
        private MailMessage _mail;

        void Start()
        {
            Initilize();
        }

        void Initilize()
        {
            _mailAddress = String.Concat(MAIL_USERNAME, (string) "@", MAIL_HOST);

            _smtpServer = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(_mailAddress, MAIL_PASSKEY) as ICredentialsByHost,
                EnableSsl = true
            };
            _smtpServer.UseDefaultCredentials = false;

            ServicePointManager.ServerCertificateValidationCallback =
                (s, certificate, chain, sslPolicyErrors) => true;
        }

        public void SendMail(string subject, string message, List<string> to, Action onSuccess = null)
        {
            StartCoroutine(ActualSend(subject, message, to, onSuccess));
        }

        IEnumerator ActualSend(string subject, string message, List<string> to, Action onSuccess)
        {
            yield return new WaitForSeconds(0.3f);

            _mail = new MailMessage();
            _mail.From = new MailAddress(_mailAddress);
            to.ForEach(_mail.To.Add);
            _mail.Subject = subject;
            _mail.Body = message;

            try
            {
                _smtpServer.SendAsync(_mail, "");
                _smtpServer.SendCompleted += (sender, args) =>
                {
                    Debug.Log(sender.ToString() + "--" + args);
                    onSuccess.SafeInvoke();
                };
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

            _mail.Dispose();
        }
    }
}
﻿/*
*
*  Copyright 2015 Netflix, Inc.
*
*     Licensed under the Apache License, Version 2.0 (the "License");
*     you may not use this file except in compliance with the License.
*     You may obtain a copy of the License at
*
*         http://www.apache.org/licenses/LICENSE-2.0
*
*     Unless required by applicable law or agreed to in writing, software
*     distributed under the License is distributed on an "AS IS" BASIS,
*     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
*     See the License for the specific language governing permissions and
*     limitations under the License.
*
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using Fido_Main.Fido_Support.Objects.Fido;


namespace Fido_Main.Notification.Email
{
    static class Email_Send
    {

        //function to send email
        public static void Send(string sTo, string sCC, string sFrom, string sSubject, string sBody, List<string> lGaugeAttachment, string sEmailAttachment)
        {
            var sErrorEmail = Object_Fido_Configs.GetAsString("fido.email.erroremail", null);
            var sFidoEmail = Object_Fido_Configs.GetAsString("fido.email.fidoemail", null);
            var sSMTPServer = Object_Fido_Configs.GetAsString("fido.email.smtpsvr", null);

            try
            {
                var mMessage = new MailMessage { IsBodyHtml = true };

                mMessage.AddRecepients(sTo, sCC);

                mMessage.AddMain(sFrom, sSubject, sBody, lGaugeAttachment);


                if (!string.IsNullOrEmpty(sEmailAttachment))
                {
                    var sAttachment = new Attachment(sEmailAttachment);

                    mMessage.Attachments.Add(sAttachment);
                }

                mMessage.SendMessage();
            }
            catch (Exception e)
            {
                Send(sErrorEmail, sFidoEmail, sFidoEmail, "Fido Error", "Fido Failed: Generic error sending email." + e, null, null);
                throw;
            }
        }

        internal static void AddRecepients(this MailMessage mMessage, string sTo, string sCC)
        {
            if (!string.IsNullOrEmpty(sTo))
            {
                mMessage.To.Add(sTo);
            }
            else
            {
                Send(sErrorEmail, "", sFidoEmail, "Fido Error", "Fido Failed: No sender specified in email.", null, null);
            }

            if (!string.IsNullOrEmpty(sCC))
            {
                mMessage.CC.Add(sCC);
            }

        }

        internal static void AddMain(this MailMessage mMessage, string sFrom, string sSubject, string sBody, List<string> lGaugeAttachment)
        {
            mMessage.From = new MailAddress(sFrom);
            mMessage.Body = sBody;
            mMessage.Subject = sSubject;

            mMessage.AlternateViews.GaugeAtt(lGaugeAttachment);
        }

        internal static void SendMessage(this MailMessage mMessage)
        {
            using (var sSMTP = new SmtpClient(sSMTPServer))
            {
                Console.WriteLine(@"Sending FIDO email.");
                var sSMTPUser = Object_Fido_Configs.GetAsString("fido.smtp.smtpuserid", string.Empty);
                var sSMTPPwd = Object_Fido_Configs.GetAsString("fido.smtp.smtppwd", string.Empty);
                sSMTP.Credentials = new NetworkCredential(sSMTPUser, sSMTPPwd);
                sSMTP.Send(mMessage);
                sSMTP.Dispose();
            }
        }

        internal static void GaugeAtt(this List<AlternateView> alternateViews, List<string> lGaugeAttachment)
        {
            if (lGaugeAttachment != null)
            {
                if (mMessage.Body != null)
                {
                    var htmlView = AlternateView.CreateAlternateViewFromString(mMessage.Body.Trim(), null, "text/html");
                    for (var i = 0; i < lGaugeAttachment.Count(); i++)
                    {

                        htmlView.LinkedResources.Add(Email_Send.ChooseGaugeAtt(lGaugeAttachment[i], i));
                    }
                    alternateViews.Add(htmlView);
                }
            }
        }

        internal static LinkedResource ChooseGaugeAtt(string gaugeAtt, int i)
        {
            switch (i)
            {
                case 0:
                    return new LinkedResource(lGaugeAttachment[i], "image/jpg") { ContentId = "totalscore" };
                    
                case 1:
                    return new LinkedResource(lGaugeAttachment[i], "image/png") { ContentId = "userscore" };
                case 2:
                    return new LinkedResource(lGaugeAttachment[i], "image/png") { ContentId = "machinescore" };
                case 3:
                    return new LinkedResource(lGaugeAttachment[i], "image/png") { ContentId = "threatscore" };
            }
        } 
    }
}

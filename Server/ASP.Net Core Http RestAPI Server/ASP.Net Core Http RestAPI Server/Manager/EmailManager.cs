using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace ASP.Net_Core_Http_RestAPI_Server
{
    public class EmailManager
    {
        static string GmailAddr = WAS_Config.getGmailAddr();
        static string GmailPassword = WAS_Config.getGmailPassword();

        static ConcurrentQueue<MailMessage> mailQueue;
        static ConcurrentQueue<SmtpClient> gmailSmtpList;

        //인증메일 체크용  (findToken, validateInfo)
        static ConcurrentDictionary<string, EmailValidationInfo> emailValidationCheck;
        
        public static async void Initialize()
        {
            emailValidationCheck = new ConcurrentDictionary<string, EmailValidationInfo>();

            mailQueue = new ConcurrentQueue<MailMessage>();
            gmailSmtpList = new ConcurrentQueue<SmtpClient>();

            for (int i = 0; i < 5; i++)
            {
                gmailSmtpList.Enqueue(CreateSMTPClient());
            }
            
            await Task.Run(async() =>
            {
                while (true)
                {
                    //보낼 message가 없다면 대기.
                    if (mailQueue.IsEmpty)
                    {
                        await Task.Delay(500);
                        continue;
                    }
                    
                    SmtpClient client;
                    if (!mailQueue.IsEmpty && gmailSmtpList.TryDequeue(out client))
                    {
                        Task.Run(async () =>
                        {
                            while (true)
                            {
                                MailMessage message;
                                if (mailQueue.TryDequeue(out message))
                                {
                                    try
                                    {
                                        //message가 왔다면 송신.
                                        client.Send(message);

                                        //보낸 메시지는 해제.
                                        message.Dispose();
                                    }
                                    catch (Exception e)
                                    {
                                        //송신에 실패한 메시지는 다시 대기.
                                        mailQueue.Enqueue(message);
                                        client.Dispose();
                                        client = CreateSMTPClient();
                                        Console.WriteLine(e.ToString());

                                        await Task.Delay(5000);
                                    }
                                }
                                //더이상 보낼 message가 없다면 종료.
                                else
                                {
                                    //사용이 끝난 client는 반환.
                                    gmailSmtpList.Enqueue(client);
                                    break;
                                }
                            }
                        }).ToString();
                    }
                    else
                        await Task.Delay(500);
                }
            });
        }

        private static SmtpClient CreateSMTPClient()
        {
            var gmailSmtp = new SmtpClient("smtp.gmail.com", 587);
            gmailSmtp.UseDefaultCredentials = false; // 시스템에 설정된 인증 정보를 사용하지 않는다.
            gmailSmtp.EnableSsl = true; // SSL을 사용한다.
            gmailSmtp.DeliveryMethod = SmtpDeliveryMethod.Network; // 이걸 하지 않으면 Gmail에 인증을 받지 못한다.
            gmailSmtp.Credentials = new System.Net.NetworkCredential(GmailAddr, GmailPassword); //Gmail 계정 정보.

            return gmailSmtp;
        }


        public static void sendGmail_SMTP(string targetMailAddr, string senderName, string mailTitle, string mailContents)
        {
            MailAddress from = new MailAddress(GmailAddr, senderName, Encoding.UTF8);
            MailAddress to = new MailAddress(targetMailAddr);

            MailMessage message = new MailMessage(from, to);
            message.SubjectEncoding = message.BodyEncoding = Encoding.UTF8;
            message.Subject = mailTitle;
            message.Body = mailContents;
            message.IsBodyHtml = false;

            mailQueue.Enqueue(message);
        }



        
        public static bool RegisterFindPasswordInfo(string findToken, EmailValidationInfo info)
        {
            if (emailValidationCheck.ContainsKey(findToken))
            {
                emailValidationCheck[findToken] = info;
            }
            else
            {
                emailValidationCheck.TryAdd(findToken, info);
            }

            return emailValidationCheck.ContainsKey(findToken);
        }

        public static EmailValidationInfo GetFindPasswordInfo(string findToken)
        {
            if (emailValidationCheck.ContainsKey(findToken))
            {
                return emailValidationCheck[findToken];
            }
            else
            {
                return null;
            }
        }

        public static void RemoveFindPasswordInfo(string findToken)
        {
            if (emailValidationCheck.ContainsKey(findToken))
            {
                EmailValidationInfo var;
                emailValidationCheck.TryRemove(findToken, out var);
            }
        }
    }

    //비밀번호 찾기용 객체 정보.
    public class EmailValidationInfo
    {
        public string FindPasswordToken;
        public string EmailAddress;
        public string EmailValidateConfirmNumber;
        public DateTime expirateTime;
        public byte currentStep;
    }
}

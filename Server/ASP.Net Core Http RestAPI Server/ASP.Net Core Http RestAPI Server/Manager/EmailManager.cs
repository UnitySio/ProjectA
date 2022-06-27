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
        static string gmailAddress = WASConfig.GetGmailAddress();
        static string gmailPassword = WASConfig.GetGmailPassword();

        static ConcurrentQueue<MailMessage> mailQueue;
        static ConcurrentQueue<SmtpClient> gmailSmtpList;

        static ConcurrentDictionary<string, EmailValidationInfo> signInValidationCheck;

        public static async void Initialize()
        {
            signInValidationCheck = new ConcurrentDictionary<string, EmailValidationInfo>();

            mailQueue = new ConcurrentQueue<MailMessage>();
            gmailSmtpList = new ConcurrentQueue<SmtpClient>();

            for (int i = 0; i < 5; i++)
                gmailSmtpList.Enqueue(CreateSMTPClient());

            CheckValidationListTask();

            await Task.Run(async () =>
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
            gmailSmtp.Credentials = new System.Net.NetworkCredential(gmailAddress, gmailPassword); //Gmail 계정 정보.

            return gmailSmtp;
        }


        public static void SendGmailSMTP(string targetMailAddress, string senderName, string mailTitle,
            string mailContents)
        {
            MailAddress from = new MailAddress(gmailAddress, senderName, Encoding.UTF8);
            MailAddress to = new MailAddress(targetMailAddress);

            MailMessage message = new MailMessage(from, to);
            message.SubjectEncoding = message.BodyEncoding = Encoding.UTF8;
            message.Subject = mailTitle;
            message.Body = mailContents;
            message.IsBodyHtml = true;

            mailQueue.Enqueue(message);
        }


        //만료기간이 1시간 넘는 오래된 항목들이 존재할 경우, 삭제한다.
        static async void CheckValidationListTask()
        {
            TimeSpan delay30Minutes = TimeSpan.FromMinutes(30);

            //설정된 주기마다 동작.
            while (true)
            {
                var list = signInValidationCheck.ToArray();
                
                foreach (var item in list)
                {
                    var itemValue = item.Value;
                    if (itemValue.expirateTime.AddHours(1) <= DateTime.UtcNow)
                        signInValidationCheck.TryRemove(itemValue.validateToken, out EmailValidationInfo var);
                }

                await Task.Delay(delay30Minutes);
            }
        }
        
        public static bool RegisterSignInInfo(string signInToken, EmailValidationInfo info)
        {
            if (signInValidationCheck.ContainsKey(signInToken))
                signInValidationCheck[signInToken] = info;
            else
                signInValidationCheck.TryAdd(signInToken, info);

            return signInValidationCheck.ContainsKey(signInToken);
        }
        
        public static EmailValidationInfo GetSignInInfo(string signInToken)
        {
            if (signInValidationCheck.ContainsKey(signInToken))
                return signInValidationCheck[signInToken];
            else
                return null;
        }
        
        public static void RemoveSignInInfo(string signInToken)
        {
            if (signInValidationCheck.ContainsKey(signInToken))
                signInValidationCheck.TryRemove(signInToken, out EmailValidationInfo var);
        }
    }

    //이메일 인증용 객체 정보.
    public class EmailValidationInfo
    {
        public string validateToken;
        public string emailAddress;
        public string emailValidateAuthNumber;
        public DateTime expirateTime;
        public byte currentStep;
    }
}
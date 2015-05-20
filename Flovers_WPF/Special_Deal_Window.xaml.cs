﻿using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp;
using iTextSharp.text.pdf;
using Flovers_WPF.Repository;
using Flovers_WPF.DataAccess;
using Flovers_WPF.DataModel;
using System.Net.Mail;
using System.Net;

namespace Flovers_WPF
{
    /// <summary>
    /// Interaction logic for Special_Deal_Window.xaml
    /// </summary>
    public partial class Special_Deal_Window : MetroWindow
    {
        public Special_Deal_Window()
        {
            InitializeComponent();
        }

        Camera_Control webcam;
        List<object> added_images = new List<object>();
        SaveFileDialog save_pdf;
        ClientsRepository oClients;
        string PDF_path;

        private async Task Initialize_Database()
        {
            DBConnection oDBConnection = new DBConnection();

            await oDBConnection.InitializeDatabase();

            oClients = new ClientsRepository(oDBConnection);
        }

        private async void spec_deal_Loaded(object sender, RoutedEventArgs e)
        {
            webcam = new Camera_Control();
            webcam.Ini_Web_Camera(ref imgVideo);
            await Initialize_Database();
        }

        private void button_startVideo_Click(object sender, RoutedEventArgs e)
        {
            webcam.Start();
        }

        private void button_stopVideo_Click(object sender, RoutedEventArgs e)
        {
            webcam.Stop();
        }

        private void button_CaptureImage_Click(object sender, RoutedEventArgs e)
        {
            imgCaptured.Source = imgVideo.Source;
        }

        private void button_SaveImage_Click(object sender, RoutedEventArgs e)
        {
            Camera_HelperClass.SaveImageCapture((BitmapSource)imgCaptured.Source);
        }

        private void add_photo_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog select_pic = new OpenFileDialog();
            if(select_pic.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                added_images.Add(select_pic.FileName);
                pictures.Items.Add(select_pic.FileName);
            }
        }

        private void delete_photo_Click(object sender, RoutedEventArgs e)
        {
            if (pictures.SelectedIndex != -1)
            {
                pictures.Items.RemoveAt(pictures.SelectedIndex);
                added_images.Remove(pictures.SelectedItem);
            }
            else
            {
                System.Windows.MessageBox.Show("выберите изображение из списка");
            }
        }

        public void Create_PDF_File(string path)
        {
            var doc = new Document();
            PdfWriter.GetInstance(doc, new FileStream(path,FileMode.Create));
            doc.Open();

            BaseFont base_font = BaseFont.CreateFont(@"H:\Курсовая\Flovers_WPF\Flovers_WPF\bin\Debug\arialn.ttf", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
            iTextSharp.text.Phrase head = new Phrase(textbox_theme.Text, new iTextSharp.text.Font(base_font,18, iTextSharp.text.Font.BOLD, new BaseColor(System.Drawing.Color.Red)));
            iTextSharp.text.Paragraph header = new iTextSharp.text.Paragraph(head);
            header.Alignment = Element.ALIGN_CENTER;
            doc.Add(header);

            iTextSharp.text.Phrase text = new Phrase(new TextRange(textbox_text.Document.ContentStart, textbox_text.Document.ContentEnd).Text,new iTextSharp.text.Font(base_font,14,iTextSharp.text.Font.NORMAL,new BaseColor(System.Drawing.Color.Black)));
            iTextSharp.text.Paragraph main_text = new iTextSharp.text.Paragraph(text);
            doc.Add(main_text);

            for (int i = 0; i < added_images.Count;i++)
            {
                iTextSharp.text.Image photo = iTextSharp.text.Image.GetInstance(added_images[i].ToString());
                doc.Add(photo);
            }
            doc.Close();
        }

        private void button_SaveAsPDF_Click(object sender, RoutedEventArgs e)
        {
            save_pdf = new SaveFileDialog();
            if(save_pdf.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Create_PDF_File(save_pdf.FileName);
                PDF_path = save_pdf.FileName;
            }
        }

        private async void button_SendMail_Click(object sender, RoutedEventArgs e)
        {
            if(PDF_path != null)
            {
                string smtpHost = "smtp.yandex.ru";
                SmtpClient client = new SmtpClient(smtpHost, 25);
                var cred = new NetworkCredential("andrewkosmynin@yandex.ru", "kosmos23051996");
                client.Credentials = cred;
                client.EnableSsl = true;

                string from = "andrewkosmynin@yandex.ru";
                string subject = textbox_theme.Text;
                string to;
                string body = "<html><body><div><div style=\"height:10%; background-color:#6DD4FF; border-radius:10px\"><p style=\"margin-left:20px; color:red\">Внимание! для тех,у кого не поддерживается HTML - снизу письма дубликат в формате PDF</p></div><div style=\"height:300px; border: 1px solid black; border-radius:10px\"><p style=\"margin-left:20px\">" + new TextRange(textbox_text.Document.ContentStart, textbox_text.Document.ContentEnd).Text.ToString() + "</p>";
                AlternateView htmlv = AlternateView.CreateAlternateViewFromString(body, null, "text/html");
                for (int i = 0; i < added_images.Count; i++)
                {
                    LinkedResource imageResource = new LinkedResource(added_images[i].ToString(), "image/jpg");
                    imageResource.ContentId = "photo" + i.ToString();
                    imageResource.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
                    htmlv.LinkedResources.Add(imageResource);

                    body += "<img style=\"margin-left:20px\" src=\"cid:" + imageResource.ContentId.ToString() + "\" alt='photo' />";
                }
                body += "</div></div></body></html>";
                Attachment pdfFile = new Attachment(save_pdf.FileName);

                List<Clients> all_clients = await oClients.Select_All_Clients_Async();
                foreach (var c in all_clients)
                {
                    to = c.email;
                    MailMessage mes = new MailMessage(from, to, subject, body);
                    mes.IsBodyHtml = true;
                    mes.SubjectEncoding = Encoding.GetEncoding(1251);
                    mes.BodyEncoding = Encoding.GetEncoding(1251);
                    mes.AlternateViews.Add(htmlv);
                    mes.Attachments.Add(pdfFile);
                    client.Send(mes);
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Сначала необходимо сохранить PDF");
            }
        }
    }
}
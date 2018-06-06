using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

using NAudio;
using NAudio.Wave;



namespace Speech
{
    public partial class Form1 : Form
    {
        private WaveIn waveIn;
        private WaveFileWriter writer;

        private const string outputFilename = "test.wav";
        private const string bufferFile = "playBuffer.wav";

        private const string apiKey = "9d55f9ea-20f0-4d83-b191-d906f8204c91";

        List<string> colorWords = new List<string>();
        List<string> carsWords = new List<string>();
        List<string> fruitWords = new List<string>();

        private readonly Random random = new Random();

        private bool recordingIsAcive = false;

        private List<string> results = new List<string>();

        public Dictionary<string, Image> colorsCollection = new Dictionary<string, Image>();
        public Dictionary<string, Image> carsCollection = new Dictionary<string, Image>();
        public Dictionary<string, Image> fruitCollection = new Dictionary<string, Image>();

        public int currentElement = 0;
        public int Points = 0;

        public Dictionary<string, Image> targetCollection = new Dictionary<string, Image>();
        List<string> targetWords = new List<string>();

        public Form1()
        {
            InitializeComponent();

            File.WriteAllText(Directory.GetCurrentDirectory() + @"/teacher/test.txt", "");

            colorWords = File.ReadAllLines(Directory.GetCurrentDirectory() + @"/words/colors.txt").ToList(); //загружаем строчки из файла
            carsWords = File.ReadAllLines(Directory.GetCurrentDirectory() + @"/words/cars.txt").ToList(); //загружаем строчки из файла
            fruitWords = File.ReadAllLines(Directory.GetCurrentDirectory() + @"/words/fruit.txt").ToList();

            colorsCollection.Add("red", Image.FromFile(Directory.GetCurrentDirectory() + @"/pictures/red.png"));
            colorsCollection.Add("black", Image.FromFile(Directory.GetCurrentDirectory() + @"/pictures/black.png"));
            colorsCollection.Add("blue", Image.FromFile(Directory.GetCurrentDirectory() + @"/pictures/blue.png"));
            colorsCollection.Add("brown", Image.FromFile(Directory.GetCurrentDirectory() + @"/pictures/brown.png"));
            colorsCollection.Add("green", Image.FromFile(Directory.GetCurrentDirectory() + @"/pictures/green.png"));
            colorsCollection.Add("grey", Image.FromFile(Directory.GetCurrentDirectory() + @"/pictures/grey.png"));
            colorsCollection.Add("orange", Image.FromFile(Directory.GetCurrentDirectory() + @"/pictures/orange.png"));
            colorsCollection.Add("pink", Image.FromFile(Directory.GetCurrentDirectory() + @"/pictures/pink.png"));
            colorsCollection.Add("purple", Image.FromFile(Directory.GetCurrentDirectory() + @"/pictures/purple.png"));

            carsCollection.Add("car", Image.FromFile(Directory.GetCurrentDirectory() + @"/pictures/car.jpg"));
            carsCollection.Add("taxi", Image.FromFile(Directory.GetCurrentDirectory() + @"/pictures/taxi.jpg"));
            carsCollection.Add("bus", Image.FromFile(Directory.GetCurrentDirectory() + @"/pictures/bus.jpg"));
            carsCollection.Add("bike", Image.FromFile(Directory.GetCurrentDirectory() + @"/pictures/bike.jpg"));
            carsCollection.Add("lorry", Image.FromFile(Directory.GetCurrentDirectory() + @"/pictures/lorry.jpeg"));
            carsCollection.Add("plane", Image.FromFile(Directory.GetCurrentDirectory() + @"/pictures/plane.jpg"));
            carsCollection.Add("ship", Image.FromFile(Directory.GetCurrentDirectory() + @"/pictures/ship.jpg"));
            carsCollection.Add("train", Image.FromFile(Directory.GetCurrentDirectory() + @"/pictures/train.jpg"));

            fruitCollection.Add("apple", Image.FromFile(Directory.GetCurrentDirectory() + @"/pictures/apple.jpg"));
            fruitCollection.Add("banana", Image.FromFile(Directory.GetCurrentDirectory() + @"/pictures/banana.jpg"));
            fruitCollection.Add("kiwi", Image.FromFile(Directory.GetCurrentDirectory() + @"/pictures/kiwi.jpg"));
            fruitCollection.Add("lemon", Image.FromFile(Directory.GetCurrentDirectory() + @"/pictures/lemon.jpg"));
            fruitCollection.Add("orange", Image.FromFile(Directory.GetCurrentDirectory() + @"/pictures/orange.jpg"));
            fruitCollection.Add("pear", Image.FromFile(Directory.GetCurrentDirectory() + @"/pictures/pear.jpg"));
            fruitCollection.Add("pineapple", Image.FromFile(Directory.GetCurrentDirectory() + @"/pictures/pineapple.jpg"));
            fruitCollection.Add("tangerine", Image.FromFile(Directory.GetCurrentDirectory() + @"/pictures/tangerine.jpg"));
        }

        void WaveInDataAvailable(object sender, WaveInEventArgs e)
        {
            writer.WriteData(e.Buffer, 0, e.BytesRecorded);
        }

        List<string> GetResultsFromYandex(byte[] record)
        {
            string postUrl = "https://asr.yandex.net/asr_xml?" + //формируем запрос
                             "uuid=01ee33cb744628b58fb536d496daa1e6&" +
                             "lang=en-US&" +
                             "key=" + apiKey + "&" +
                             "topic=queries";

            string response = PostMethod(record, postUrl); //посылаем наш звук в фармате массива байтов

            var strings = Regex.Matches(response, "(?<=\">)(.*)(?=<)"); //парсим то что пришло с сайта

            results = new List<string>();

            foreach (Match item in strings)
            {
                results.Add(item.ToString());
            }

            return results;
        }

        private void StartRecording()
        {
            waveIn = new WaveIn(); //создаём экземпляр класса, который отвечает за запись на микрофон
            waveIn.DeviceNumber = 0;
            waveIn.DataAvailable += WaveInDataAvailable; //подписываемся на событие, т.е. пока есть данные будет выполняться метод WaveInDataAvailable
            waveIn.RecordingStopped += WaveInRecordingStopped; //подписываемся на событие, т.е. когда мы остановим запись, то сработает метод WaveInRecordingStopped
            waveIn.WaveFormat = new WaveFormat(96000, 2);
            writer = new WaveFileWriter(outputFilename, waveIn.WaveFormat);
            waveIn.StartRecording();
        }

        void WaveInRecordingStopped(object sender, EventArgs e)
        {
            waveIn.Dispose(); //останавливаем запись
            waveIn = null;
            writer.Close(); //сбрасываем экземпляр класс, который отвечает за запись
            writer = null;

            byte[] record = File.ReadAllBytes(outputFilename); //загружаем нашу запись из файла
            results = GetResultsFromYandex(record); //получаем варианты расшифровки того что мы мсказали

            //textBox1.Text = results[0];

            foreach (var line in results)
            {
                textBox1.Text += line + "\r\n"; //отображаем результаты в текстовом поле
            }

            button1.Text = "Начать запись"; //сбрасываем название кнопки
            button1.Enabled = true;//и делаем её активной

            if (results.Exists(x => x.ToLower() == textBox2.Text.ToLower())) //если есть совпадения (при помощи linq выражений)
            {
                label2.Text = "Правильно!";
                Points++;
            }
            else
            {
                label2.Text = "Неправильно!";
            }
            File.AppendAllText(Directory.GetCurrentDirectory() + @"/teacher/test.txt", textBox2.Text + "\r\n"+"\r\n");
            File.AppendAllLines(Directory.GetCurrentDirectory() + @"/teacher/test.txt", results);
            File.AppendAllText(Directory.GetCurrentDirectory() + @"/teacher/test.txt", "------------------" + "\r\n");
            File.Delete(outputFilename);
            File.Delete(bufferFile);

        }

        private void StopRecording()
        {
            waveIn.StopRecording(); //на этом моменте вызывается WaveInRecordingStopped
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //тут и запись и остановка на одну кнопку
            if (!recordingIsAcive)
            {
                StartRecording();

                recordingIsAcive = true;
                button1.Text = "Остановить запись";
            }
            else
            {
                StopRecording();

                recordingIsAcive = false;
                button1.Enabled = false;
                button1.Text = "Загружается результат";

                textBox1.Clear();
                button1.Visible = false;
                label2.Visible = true;
            }
        }

        private string PostMethod(byte[] bytes, string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";

            request.ContentType = "audio/x-wav";
            request.ContentLength = bytes.Length;

            using (var newStream = request.GetRequestStream())
            {
                newStream.Write(bytes, 0, bytes.Length);
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string responseToString = "";

            if (response != null)
            {
                var streamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                responseToString = streamReader.ReadToEnd();
            }

            return responseToString;
        }

        private static byte[] SendGetRequest(string url, CookieContainer cookie = null, int timeout = 0)
        {
            string content = "";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            if (cookie != null)
                request.CookieContainer = cookie;

            request.ProtocolVersion = HttpVersion.Version10;
            request.Timeout = timeout != 0 ? timeout : 60000;
            request.ContinueTimeout = 50000;
            request.ReadWriteTimeout = 50000;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            var bytes = default(byte[]);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                StreamReader stream = new StreamReader(response.GetResponseStream());
                using (var memstream = new MemoryStream())
                {
                    var buffer = new byte[512];
                    var bytesRead = default(int);
                    while ((bytesRead = stream.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
                        memstream.Write(buffer, 0, bytesRead);
                    bytes = memstream.ToArray();
                }

                stream.Close();
            }
            return bytes;
        }

        private void SayTheText(string text)
        {
            //speaker = jane | oksana | alyss | omazh | zahar | ermil
            //emotion = good | neutral | evil
            //speed 0.1 .. 3

            string url = $"https://tts.voicetech.yandex.net/generate" + //формируем запрос
                         $"?key={apiKey}" +
                         $"&text={text}" +
                         $"&format=wav" +
                         $"&quality=hi" +
                         $"&lang=en-US" +
                         $"&speaker=zahar" +
                         $"&speed=1.1" +
                         $"&emotion=good";

            var response = SendGetRequest(url); //получаем ответ с сайта

            
            File.WriteAllBytes(bufferFile, response); //записываем в файл то что получили

            SoundPlayer snd = new SoundPlayer(bufferFile); //воспроизводим звук из файла
            snd.Play();
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SayTheText(textBox2.Text); //произносим то что написано в тектовом поле
        }

        private void button5_Click(object sender, EventArgs e)
        {

            if (targetWords.Count > currentElement)
            {
                label2.Visible = false;
                button1.Visible = true;
                label3.Visible = true;
                textBox2.Text = targetWords[currentElement];//берём слово из словаря
                label3.Text = $"{currentElement + 1} / {targetWords.Count}";
            }
            else
            {
                label4.Visible = true;
                label4.Text = $"Правильно {Points} из {targetWords.Count}";
                label3.Visible = false;
                button5.Visible = false;
                button4.Visible = false;
                button1.Visible = false;
                label2.Visible = false;
                textBox2.Visible = false;
                label1.Visible = false;
                pictureBox1.Visible = false;
                button6.Visible = true;
            }

            currentElement++;
         // pictureBox1.Image = colors[textBox2.Text];
            if (targetCollection.ContainsKey(textBox2.Text))
            {
                targetCollection.TryGetValue(textBox2.Text, out Image value);
                pictureBox1.Image = value;
            }

            if (currentElement == targetWords.Count)
            {
                button5.Text = "Завершить";
            }

        }

        private void button2_Click(object sender, EventArgs e)      //категория 1
        {
            targetCollection = colorsCollection;
            targetWords = colorWords;
            ShowInterface();
        }

        private void button3_Click(object sender, EventArgs e)      //категория 2
        {
            targetCollection = carsCollection;
            targetWords = carsWords;
           // File.WriteAllText(Directory.GetCurrentDirectory() + @"/teacher/test.txt", "");
            ShowInterface();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            targetCollection = fruitCollection;
            targetWords = fruitWords;
         //   File.WriteAllText(Directory.GetCurrentDirectory() + @"/teacher/test.txt", "");
            ShowInterface();
        }

        private void button6_Click(object sender, EventArgs e)      //вернуться
        {
            pictureBox1.Image = null;
            textBox2.Text = "";
            Points = 0;
            currentElement = 0;
            button5.Text = "Следующее слово";

            button2.Visible = true;
            button3.Visible = true;
            button7.Visible = true;
            pictureBox1.Visible = false;
            label1.Visible = false;
            textBox2.Visible = false;
            button6.Visible = false;
            button5.Visible = false;
            button4.Visible = false;
            label2.Visible = false;
            button1.Visible = false;
            label3.Visible = false;
            label4.Visible = false;

        }

        private void ShowInterface()
        {
            button2.Visible = false;
            button3.Visible = false;
            button7.Visible = false;
            pictureBox1.Visible = true;
            label1.Visible = true;
            textBox2.Visible = true;
            button5.Visible = true;
            button4.Visible = true;
            //label2.Visible = true;
           // button1.Visible = true;
            label3.Visible = true;
            button6.Visible = false;
            label3.Text = $"{0} / {0}";
        }

    }
}

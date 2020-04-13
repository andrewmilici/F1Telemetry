using F1Telemetry.Packets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SpriteFontPlus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace F1Telemetry
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class F1Telemetry : Game
    {

        List<Tuple<float, float>> currentLapTiming;
        List<Tuple<float, float>> bestLapTiming;



        float lastLapDistance = 0;

        string lapStatus = "";
        long packetCount = 0;
        string currentGear = "N";
        bool ledRPMStrip1 = false;
        bool ledRPMStrip2 = false;
        bool ledRPMStrip3 = false;
        string rpmPercent = "";
        ushort currentRPM = 0;
        string currentLap = "";
        string lastLap = "";
        string currentDelta = "-00.23";
        bool testing = true;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        SpriteFont arial10;
        SpriteFont arial100;
        SpriteFont arial70;
        SpriteFont arial50;
        SpriteFont arial16;
        Texture2D dash;
        Texture2D redLed;
        Texture2D greenLed;

        Song larry;
        Texture2D blueLed;

        public F1Telemetry()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            UdpClient socket = new UdpClient(20777);
            socket.BeginReceive(new AsyncCallback(OnUdpData), socket);
            this.IsMouseVisible = true;
            if (System.IO.File.Exists(@"C:\Users\Andrew\Desktop\f1\output.txt"))
                System.IO.File.Delete(@"C:\Users\Andrew\Desktop\f1\output.txt");

            base.Initialize();
        }

        private void StartNewLap()
        {
            /*Save it to file
             * was this the best lap? Copy to bestlap
             * clear currentLap
             */

            if (currentLapTiming != null)
            {
                var sb = new StringBuilder();
                foreach (var item in currentLapTiming)
                {
                    sb.AppendLine($"{ item.Item1.ToString() },{ item.Item2.ToString() }");
                }
                Task.Run(() => WriteTextAsync(@"C:\Users\Andrew\Desktop\F1\output_" + DateTime.Now.Ticks.ToString() + ".txt", sb.ToString()));

                currentLapTiming = null;
            }

            currentLapTiming = new List<Tuple<float, float>>();
        }

        void OnUdpData(IAsyncResult result)
        {
            UdpClient socket = result.AsyncState as UdpClient;
            IPEndPoint source = new IPEndPoint(0, 0);
            byte[] message = socket.EndReceive(result, ref source);

            var ph = new PacketHeader(message);

            switch (ph.m_packetId)
            {
                //case 1:
                //    var sessionData = new PacketSessionData();
                //    sessionData.RawData = ph.RawData;
                //    sessionData.LoadData();
                //    break;
                case 2:
                    var lapData = new PacketLapData();
                    lapData.RawData = ph.RawData;
                    lapData.LoadData();

                    currentLap = lapData.m_lapData[lapData.m_header.m_playerCarIndex].m_currentLapNum.ToString();
                    var lastLapTS = TimeSpan.FromSeconds(lapData.m_lapData[lapData.m_header.m_playerCarIndex].m_lastLapTime);
                    lastLap = $"{ lastLapTS.Minutes }:{ lastLapTS.Seconds }.{ lastLapTS.Milliseconds }";

                    if (lapData.m_lapData[lapData.m_header.m_playerCarIndex].m_lapDistance > 0)
                    {
                        if (lastLapDistance < 0)
                        {
                            //NEW LAP
                            MediaPlayer.Play(larry);
                            StartNewLap();
                        }
                        else
                        {
                            var diff = lastLapDistance - lapData.m_lapData[lapData.m_header.m_playerCarIndex].m_lapDistance;
                            if (diff > 1000)
                            {
                                //NEW LAP
                                
                                MediaPlayer.Play(larry);
                                StartNewLap();
                            }
                        }
                    }

                    if (currentLapTiming == null)
                        currentLapTiming = new List<Tuple<float, float>>();

                    currentLapTiming.Add(new Tuple<float, float>(lapData.m_lapData[lapData.m_header.m_playerCarIndex].m_lapDistance, lapData.m_lapData[lapData.m_header.m_playerCarIndex].m_currentLapTime));



                    lastLapDistance = lapData.m_lapData[lapData.m_header.m_playerCarIndex].m_lapDistance;

                    //lastLap = lapData.m_lapData[0].m_lapDistance
                    //lastLap = lapData.m_lapData[0].m_lapDistance.ToString();
                    //Task.Run(() => WriteTextAsync(@"C:\Users\Andrew\Desktop\f1\output.txt", lapData.m_lapData[0].m_lapDistance.ToString() + Environment.NewLine));

                    packetCount++;
                    break;
                case 6:
                    var tele = new PacketCarTelemetryData();
                    tele.RawData = ph.RawData;
                    tele.LoadData();
                    currentGear = tele.m_carTelemetryData[tele.m_header.m_playerCarIndex].m_gear.ToString();
                    currentRPM = tele.m_carTelemetryData[tele.m_header.m_playerCarIndex].m_engineRPM;
                    var rpm = tele.m_carTelemetryData[tele.m_header.m_playerCarIndex].m_revLightsPercent;
                    rpmPercent = rpm.ToString();
                    ledRPMStrip1 = false;
                    ledRPMStrip2 = false;
                    ledRPMStrip3 = false;

                    if (rpm > 25)
                        ledRPMStrip1 = true;

                    if (rpm > 50)
                        ledRPMStrip2 = true;

                    if (rpm > 75)
                        ledRPMStrip3 = true;

                    //lastTelemetry = tele;
                    break;

            }

            socket.BeginReceive(new AsyncCallback(OnUdpData), socket);
        }

        static async Task WriteTextAsync(string filePath, string text)
        {
            byte[] encodedText = System.Text.Encoding.Unicode.GetBytes(text);

            using (FileStream sourceStream = new FileStream(filePath,
                FileMode.Append, FileAccess.Write, FileShare.None,
                bufferSize: 4096, useAsync: true))
            {
                await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
            };
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            dash = Content.Load<Texture2D>("Dash");
            redLed = Content.Load<Texture2D>("red");
            blueLed = Content.Load<Texture2D>("blue");
            greenLed = Content.Load<Texture2D>("green");
            larry = Content.Load<Song>("ye");
            var arialBuffer = File.ReadAllBytes(@"C:\\Windows\\Fonts\arial.ttf");
            arial100 = TtfFontBaker.Bake(arialBuffer, 100, 1024, 1024, new[] { CharacterRange.BasicLatin, CharacterRange.Latin1Supplement, CharacterRange.LatinExtendedA, CharacterRange.Cyrillic }).CreateSpriteFont(GraphicsDevice);
            arial10 = TtfFontBaker.Bake(arialBuffer, 10, 1024, 1024, new[] { CharacterRange.BasicLatin, CharacterRange.Latin1Supplement, CharacterRange.LatinExtendedA, CharacterRange.Cyrillic }).CreateSpriteFont(GraphicsDevice);
            arial70 = TtfFontBaker.Bake(arialBuffer, 70, 1024, 1024, new[] { CharacterRange.BasicLatin, CharacterRange.Latin1Supplement, CharacterRange.LatinExtendedA, CharacterRange.Cyrillic }).CreateSpriteFont(GraphicsDevice);
            arial50 = TtfFontBaker.Bake(arialBuffer, 50, 1024, 1024, new[] { CharacterRange.BasicLatin, CharacterRange.Latin1Supplement, CharacterRange.LatinExtendedA, CharacterRange.Cyrillic }).CreateSpriteFont(GraphicsDevice);
            arial16 = TtfFontBaker.Bake(arialBuffer, 16, 1024, 1024, new[] { CharacterRange.BasicLatin, CharacterRange.Latin1Supplement, CharacterRange.LatinExtendedA, CharacterRange.Cyrillic }).CreateSpriteFont(GraphicsDevice);

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(183, 183, 183));

            spriteBatch.Begin();



            var leftDash = (GraphicsDevice.Viewport.Bounds.Width - 450) / 2;
            var topDash = (GraphicsDevice.Viewport.Bounds.Height - 300) / 2;
            spriteBatch.Draw(dash, new Rectangle(leftDash, topDash, 450, 300), Color.White);

            var mouseState = Mouse.GetState();
            spriteBatch.DrawString(arial10, $"{mouseState.X},{mouseState.Y}", new Vector2(10, 10), Color.White);

            spriteBatch.DrawString(arial100, lapStatus, new Vector2(275, 15), new Color(81, 243, 228));

            spriteBatch.DrawString(arial100, currentGear.ToString(), new Vector2(365, 180), new Color(81, 243, 228));
            spriteBatch.DrawString(arial70, currentRPM.ToString(), new Vector2(610 - arial70.MeasureString(currentRPM.ToString()).X, 123), new Color(221, 116, 130));
            spriteBatch.DrawString(arial70, currentLap, new Vector2(270, 123), new Color(81, 243, 228));
            spriteBatch.DrawString(arial50, lastLap, new Vector2(610 - arial50.MeasureString(lastLap).X, 310), new Color(81, 243, 228));
            spriteBatch.DrawString(arial50, currentDelta, new Vector2(255, 310), new Color(81, 243, 228));


            var rect1 = HelperMethods.DrawRectangle(GraphicsDevice, spriteBatch, new Rectangle(leftDash + 25, topDash + 75, 35, 180), Color.White);
            var mainRPMRect = new Rectangle(leftDash + 27, topDash + 77, 31, 176);
            var rect2 = HelperMethods.DrawRectangle(GraphicsDevice, spriteBatch, mainRPMRect, Color.Black);
            var rpmMax = 13600;
            var rpmMin = 5000;
            float percent = (float)(currentRPM - rpmMin) / (float)(rpmMax - rpmMin);
            Rectangle rect = mainRPMRect;

            // Calculate area for drawing the progress.
            rect.Height = (int)((float)rect.Height * percent);
            rect.Y = (mainRPMRect.Height - rect.Height) + mainRPMRect.Y;
            var rect3 = HelperMethods.DrawRectangle(GraphicsDevice, spriteBatch, rect, Color.White);

            spriteBatch.DrawString(arial16, currentRPM.ToString(), new Vector2(195, 350), Color.White);

            //Data Labels
            spriteBatch.DrawString(arial16, "Gear", new Vector2(375, 270), Color.White);
            spriteBatch.DrawString(arial16, "RPM", new Vector2(510, 190), Color.White);
            spriteBatch.DrawString(arial16, "RPM", new Vector2(leftDash + 27, topDash + 50), Color.White);
            spriteBatch.DrawString(arial16, "Lap", new Vector2(270, 190), Color.White);
            spriteBatch.DrawString(arial16, "Last Lap", new Vector2(500, 360), Color.White);
            spriteBatch.DrawString(arial16, "Delta", new Vector2(290, 360), Color.White);


            var offset = 27;
            var ledGap = 27;

            if (ledRPMStrip1)
            {
                spriteBatch.Draw(greenLed, new Rectangle(leftDash + offset, topDash + 15, 20, 20), Color.White);
                spriteBatch.Draw(greenLed, new Rectangle(leftDash + offset + (ledGap), topDash + 15, 20, 20), Color.White);
                spriteBatch.Draw(greenLed, new Rectangle(leftDash + offset + (ledGap * 2), topDash + 15, 20, 20), Color.White);
                spriteBatch.Draw(greenLed, new Rectangle(leftDash + offset + (ledGap * 3), topDash + 15, 20, 20), Color.White);
                spriteBatch.Draw(greenLed, new Rectangle(leftDash + offset + (ledGap * 4), topDash + 15, 20, 20), Color.White);
            }

            if (ledRPMStrip2)
            {
                spriteBatch.Draw(redLed, new Rectangle(leftDash + offset + (ledGap * 5), topDash + 15, 20, 20), Color.White);
                spriteBatch.Draw(redLed, new Rectangle(leftDash + offset + (ledGap * 6), topDash + 15, 20, 20), Color.White);
                spriteBatch.Draw(redLed, new Rectangle(leftDash + offset + (ledGap * 7), topDash + 15, 20, 20), Color.White);
                spriteBatch.Draw(redLed, new Rectangle(leftDash + offset + (ledGap * 8), topDash + 15, 20, 20), Color.White);
                spriteBatch.Draw(redLed, new Rectangle(leftDash + offset + (ledGap * 9), topDash + 15, 20, 20), Color.White);
            }

            if (ledRPMStrip3)
            {
                spriteBatch.Draw(blueLed, new Rectangle(leftDash + offset + (ledGap * 10), topDash + 15, 20, 20), Color.White);
                spriteBatch.Draw(blueLed, new Rectangle(leftDash + offset + (ledGap * 11), topDash + 15, 20, 20), Color.White);
                spriteBatch.Draw(blueLed, new Rectangle(leftDash + offset + (ledGap * 12), topDash + 15, 20, 20), Color.White);
                spriteBatch.Draw(blueLed, new Rectangle(leftDash + offset + (ledGap * 13), topDash + 15, 20, 20), Color.White);
                spriteBatch.Draw(blueLed, new Rectangle(leftDash + offset + (ledGap * 14), topDash + 15, 20, 20), Color.White);
            }



            spriteBatch.End();
            // TODO: Add your drawing code here
            rect1.Dispose();
            rect2.Dispose();
            rect3.Dispose();

            base.Draw(gameTime);
        }
    }
}

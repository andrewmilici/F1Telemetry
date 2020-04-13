using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace F1Telemetry.Packets
{
    public class CarTelemetryData
    {
        public ushort m_speed { get; set; }         //Speed of car in kilometres per hour
        public float m_throttle { get; set; }       //Amount of throttle applied (0.0 to 1.0)
        public float m_steer { get; set; }          //Steering (-1.0 (full lock left) to 1.0 (full lock right))
        public float m_brake { get; set; }          //Amount of brake applied (0.0 to 1.0)
        public byte m_clutch { get; set; }          //Amount of clutch applied (0 to 100)
        public sbyte m_gear { get; set; }           //Gear selected (1-8, N=0, R=-1)
        public ushort m_engineRPM { get; set; }     //Engine RPM
        public byte m_drs { get; set; }             //0 = off, 1 = on
        public byte m_revLightsPercent { get; set; } //Rev lights indicator (percentage)
        public ushort[] m_brakesTemperature { get; set; } //Brakes temperature (celsius)
        public ushort[] m_tyresSurfaceTemperature { get; set; } //Tyres surface temperature (celsius)
        public ushort[] m_tyresInnerTemperature { get; set; } //Tyres inner temperature (celsius)
        public ushort m_engineTemperature { get; set; } //Engine temperature (celsius)
        public float[] m_tyresPressure { get; set; } //Tyres pressure (PSI)
        public byte[] m_surfaceType { get; set; } //Driving surface, see appendices

        public CarTelemetryData()
        {
            m_brakesTemperature = new ushort[4];
            m_tyresSurfaceTemperature = new ushort[4];
            m_tyresInnerTemperature = new ushort[4];
            m_tyresPressure = new float[4];
            m_surfaceType = new byte[4];
        }
    }

    public class PacketCarTelemetryData
    {
        public PacketHeader m_header { get; set; }
        public CarTelemetryData[] m_carTelemetryData { get; set; }
        public uint m_buttonStatus { get; set; }
        public byte[] RawData { get; set; }
        public PacketCarTelemetryData()
        {
            m_carTelemetryData = new CarTelemetryData[20];

        }

        public PacketCarTelemetryData(byte[] rawData)
        {
            m_carTelemetryData = new CarTelemetryData[20];
            RawData = rawData;
            LoadData();
        }

        public void LoadData()
        {
            m_header = new PacketHeader();
            m_header.RawData = RawData;
            var currentPos = m_header.LoadData();

            using (var br = new BinaryReader(new MemoryStream(RawData)))
            {
                br.BaseStream.Position = currentPos;

                for (int i = 0; i < m_carTelemetryData.GetUpperBound(0); i++)
                {
                    m_carTelemetryData[i] = new CarTelemetryData();
                    m_carTelemetryData[i].m_speed = br.ReadUInt16();
                    m_carTelemetryData[i].m_throttle = br.ReadSingle();
                    m_carTelemetryData[i].m_steer = br.ReadSingle();
                    m_carTelemetryData[i].m_brake = br.ReadSingle();
                    m_carTelemetryData[i].m_clutch = br.ReadByte();
                    m_carTelemetryData[i].m_gear = br.ReadSByte();
                    m_carTelemetryData[i].m_engineRPM = br.ReadUInt16();
                    m_carTelemetryData[i].m_drs = br.ReadByte();
                    m_carTelemetryData[i].m_revLightsPercent = br.ReadByte();

                    for (int x = 0; x < 4; x++)
                        m_carTelemetryData[i].m_brakesTemperature[x] = br.ReadUInt16();

                    for (int x = 0; x < 4; x++)
                        m_carTelemetryData[i].m_tyresSurfaceTemperature[x] = br.ReadUInt16();

                    for (int x = 0; x < 4; x++)
                        m_carTelemetryData[i].m_tyresInnerTemperature[x] = br.ReadUInt16();

                    m_carTelemetryData[i].m_engineTemperature = br.ReadUInt16();

                    for (int x = 0; x < 4; x++)
                        m_carTelemetryData[i].m_tyresPressure[x] = br.ReadSingle();

                    for (int x = 0; x < 4; x++)
                        m_carTelemetryData[i].m_surfaceType[x] = br.ReadByte();


                }
            }
        }
    }
}

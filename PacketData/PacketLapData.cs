using F1Telemetry.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F1Telemetry.Packets
{
    public class LapData
    {
        public float m_lastLapTime { get; set; }            // Last lap time in seconds
        public float m_currentLapTime { get; set; } // Current time around the lap in seconds
        public float m_bestLapTime { get; set; }        // Best lap time of the session in seconds
        public float m_sector1Time { get; set; }        // Sector 1 time in seconds
        public float m_sector2Time { get; set; }        // Sector 2 time in seconds
        public float m_lapDistance { get; set; }        // Distance vehicle is around current lap in metres – could
                                                        // be negative if line hasn’t been crossed yet
        public float m_totalDistance { get; set; }      // Total distance travelled in session in metres – could
                                                        // be negative if line hasn’t been crossed yet
        public float m_safetyCarDelta { get; set; }        // Delta in seconds for safety car
        public byte m_carPosition { get; set; }    // Car race position
        public byte m_currentLapNum { get; set; }      // Current lap number
        public byte m_pitStatus { get; set; }              // 0 = none, 1 = pitting, 2 = in pit area
        public byte m_sector { get; set; }                 // 0 = sector1, 1 = sector2, 2 = sector3
        public byte m_currentLapInvalid { get; set; }      // Current lap invalid - 0 = valid, 1 = invalid
        public byte m_penalties { get; set; }              // Accumulated time penalties in seconds to be added
        public byte m_gridPosition { get; set; }           // Grid position the vehicle started the race in
        public byte m_driverStatus { get; set; }           // Status of driver - 0 = in garage, 1 = flying lap
                                                           // 2 = in lap, 3 = out lap, 4 = on track
        public byte m_resultStatus { get; set; }          // Result status - 0 = invalid, 1 = inactive, 2 = active
                                                          // 3 = finished, 4 = disqualified, 5 = not classified
                                                          // 6 = retired
    }

    public class PacketLapData
    {
        public byte[] RawData { get; set; }
        public PacketHeader m_header { get; set; }
        public LapData[] m_lapData { get; set; }

        public PacketLapData()
        {
            m_lapData = new LapData[20];

        }

        public PacketLapData(byte[] rawData)
        {
            m_lapData = new LapData[20];
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

                for (int i = 0; i < m_lapData.GetUpperBound(0); i++)
                {
                    m_lapData[i] = new LapData();

                    m_lapData[i].m_lastLapTime = br.ReadSingle();
                    m_lapData[i].m_currentLapTime = br.ReadSingle();
                    m_lapData[i].m_bestLapTime = br.ReadSingle();
                    m_lapData[i].m_sector1Time = br.ReadSingle();
                    m_lapData[i].m_sector2Time = br.ReadSingle();
                    m_lapData[i].m_lapDistance = br.ReadSingle();
                    m_lapData[i].m_totalDistance = br.ReadSingle();
                    m_lapData[i].m_safetyCarDelta = br.ReadSingle();
                    m_lapData[i].m_carPosition = br.ReadByte();
                    m_lapData[i].m_currentLapNum = br.ReadByte();
                    m_lapData[i].m_pitStatus = br.ReadByte();
                    m_lapData[i].m_sector = br.ReadByte();
                    m_lapData[i].m_currentLapInvalid = br.ReadByte();
                    m_lapData[i].m_penalties = br.ReadByte();
                    m_lapData[i].m_gridPosition = br.ReadByte();
                    m_lapData[i].m_driverStatus = br.ReadByte();
                    m_lapData[i].m_resultStatus = br.ReadByte();


                }
            }
        }
    }
}

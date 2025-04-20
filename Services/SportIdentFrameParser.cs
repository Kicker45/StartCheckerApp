using System;

namespace StartCheckerApp.Services
{
    public enum SiCommand : byte
    {
        SICard5Detected = 0xE5,
        SICard6Detected = 0xE6,
        SICard8Detected = 0xE8,
        SICardRemoved = 0xF0
    }

    public static class SportIdentFrameParser
    {
        public static bool TryParseSiFrame(byte[] frame, int length, out int siid)
        {
            siid = 0;

            if (length < 5 || frame[0] != 0x02 || frame[length - 1] != 0x03)
                return false;

            var command = (SiCommand)frame[1];

            try
            {
                switch (command)
                {
                    case SiCommand.SICard5Detected:
                        {
                            int cardSeries = frame[6];
                            int shortId = frame[7] << 8 | frame[8];
                            siid = shortId + (cardSeries > 1 ? 100000 * cardSeries : 0);
                            return true;
                        }

                    case SiCommand.SICard6Detected:
                    case SiCommand.SICard8Detected:
                        {
                            int cardSeries = frame[5];
                            int id = frame[6] << 16 | frame[7] << 8 | frame[8];
                            siid = id; // série lze připočítat, pokud to odpovídá
                            return true;
                        }

                    case SiCommand.SICardRemoved:
                        siid = 0;
                        return false;

                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}

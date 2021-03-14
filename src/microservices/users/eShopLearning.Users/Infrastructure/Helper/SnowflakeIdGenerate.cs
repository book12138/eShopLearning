using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace eShopLearning.Users.Infrastructure.Helper
{
    /// <summary>
    /// 雪花id生成
    /// </summary>
    public class SnowflakeIdGenerate
    {
        //机器ID
        private static long workerId;
        private static long twepoch = 687888001020L; //唯一时间，这是一个避免重复的随机量，自行设定不要大于当前时间戳
        private static long sequence = 0L;
        private static int workerIdBits = 4; //机器码字节数。4个字节用来保存机器码(定义为Long类型会出现，最大偏移64位，所以左移64位没有意义)
        public static long maxWorkerId = -1L ^ -1L << workerIdBits; //最大机器ID
        private static int sequenceBits = 10; //计数器字节数，10个字节用来保存计数码
        private static int workerIdShift = sequenceBits; //机器码数据左移位数，就是后面计数器占用的位数
        private static int timestampLeftShift = sequenceBits + workerIdBits; //时间戳左移动位数就是机器码和计数器总字节数
        public static long sequenceMask = -1L ^ -1L << sequenceBits; //一微秒内可以产生计数，如果达到该值则等到下一微妙在进行生成
        private long lastTimestamp = -1L;

        /// <summary>
        /// 机器码
        /// </summary>
        /// <param name="workerId"></param>
        public SnowflakeIdGenerate(long workerId)
        {
            if (workerId > maxWorkerId || workerId < 0)
                throw new Exception(string.Format("worker Id can't be greater than {0} or less than 0 ", workerId));
            SnowflakeIdGenerate.workerId = workerId;
        }

        public long NextId()
        {
            lock (this)
            {
                long timestamp = timeGen();
                if (this.lastTimestamp == timestamp)
                { //同一微妙中生成ID
                    SnowflakeIdGenerate.sequence = (SnowflakeIdGenerate.sequence + 1) & SnowflakeIdGenerate.sequenceMask; //用&运算计算该微秒内产生的计数是否已经到达上限
                    if (SnowflakeIdGenerate.sequence == 0)
                    {
                        //一微妙内产生的ID计数已达上限，等待下一微妙
                        timestamp = tillNextMillis(this.lastTimestamp);
                    }
                }
                else
                { //不同微秒生成ID
                    SnowflakeIdGenerate.sequence = 0; //计数清0
                }
                if (timestamp < lastTimestamp)
                { //如果当前时间戳比上一次生成ID时时间戳还小，抛出异常，因为不能保证现在生成的ID之前没有生成过
                    throw new Exception(string.Format("Clock moved backwards.  Refusing to generate id for {0} milliseconds",
                        this.lastTimestamp - timestamp));
                }
                this.lastTimestamp = timestamp; //把当前时间戳保存为最后生成ID的时间戳
                long nextId = (timestamp - twepoch << timestampLeftShift) | SnowflakeIdGenerate.workerId << SnowflakeIdGenerate.workerIdShift | SnowflakeIdGenerate.sequence;
                return nextId;
            }
        }

        /// <summary>
        /// 获取下一微秒时间戳
        /// </summary>
        /// <param name="lastTimestamp"></param>
        /// <returns></returns>
        private long tillNextMillis(long lastTimestamp)
        {
            long timestamp = timeGen();
            while (timestamp <= lastTimestamp)
            {
                timestamp = timeGen();
            }
            return timestamp;
        }

        /// <summary>
        /// 生成当前时间戳
        /// </summary>
        /// <returns></returns>
        private long timeGen()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }
    }

    /// <summary>
    /// 分布式自增算法 - 产生唯一ID
    /// </summary>
    public static class SnowFlakeAlg
    {
        /// <summary>
        /// 锁住的资源对象
        /// </summary>
        private static object lockItem = new object();

        /// <summary>
        /// 上一个时间戳- 进行比较用
        /// </summary>
        private static long lastDateTimeStamp = 0L;

        /// <summary>
        /// Sequence 序列号 允许0~4095
        /// </summary>
        private static long sequence = 0L;

        public static long GetGuid()
        {
            //异步情况下，使用Lock 确保产生唯一ID
            lock (lockItem)
            {
                long result = 0L;
                //1. 41bit
                long timelong = SnowFlakeAlg.GetTimeSpan.GetTimeSpanReturnLong();
                //2. 10bit
                long macSn = SnowFlakeAlg.GetMacAddress.GetTenBitMacAddress();
                //3. 12bit
                sequence++;

                //seq == 0 表示 同一秒内已经被排了4095次，Seq 已经用尽，切换至下一秒
                if (timelong == lastDateTimeStamp)
                {
                    if (true == SnowFlakeAlg.GetSequence.checkSeq(sequence))
                    {
                        //取得下一微秒的Long值
                        timelong = SnowFlakeAlg.GetTimeSpan.GetTimeSpanReturnNextSecondLong();
                    }
                }
                else//不同微秒下
                {
                    sequence = 0;//归0
                }
                //纪录本次的TimeStamp
                lastDateTimeStamp = timelong;

                //41bit 
                result = ((timelong) << 22) | macSn << 12 | sequence;

                return result;
            }

        }

        #region Method

        #region Mac Function 10bit


        private class GetMacAddress
        {
            /// <summary>
            /// Ascii对应表
            /// </summary>
            private static Dictionary<char, int> dicAscii = new Dictionary<char, int>()
            {
                {'0' , 0},{ '1' ,1} , {'2' , 2},{ '3' ,3} , {'4' , 4},{ '5' ,5} , {'6' , 6},{ '7' ,7} , {'8' , 8},{ '9' ,9} , {'A' , 10},{ 'B' ,11} , {'C' , 12},{ 'D' ,13} , { 'E' ,14} ,{'F' , 15}
            };

            /// <summary>
            /// 取得10位的网络卡位址
            /// </summary>
            /// <returns></returns>
            public static long GetTenBitMacAddress()
            {
                //取得网卡Libary - 取得本机器所有网络卡
                NetworkInterface[] macs = NetworkInterface.GetAllNetworkInterfaces();

                //取得电脑上的 Ethernet 的 MAC Address ，第一个抓到的实例网卡
                var result = macs.Where(o => o.NetworkInterfaceType == NetworkInterfaceType.Ethernet).FirstOrDefault();

                //没有网卡
                if (null == result)
                {
                    return 0;
                }   //return 0L;
                else//有网卡则进行计算
                {
                    //※逻辑 ： SnowFlake 算法取10bit ，实例网卡为 12Byte EX: E0-3F-49-4D-01-1C
                    //          取最后两个Byte(2 * 8) 进行6bit位移，取10Bit 

                    //String -> ASCII 
                    byte[] macDecByte = System.Text.Encoding.ASCII.GetBytes(result.GetPhysicalAddress().ToString());

                    //左边
                    int left = AscIIToInt((char)Convert.ToInt32(macDecByte[8])) * 16 + AscIIToInt((char)Convert.ToInt32(macDecByte[9])) * 1 << 8;//=>x的位   x x x x x x x x o o o o o o o o
                    int right = AscIIToInt((char)Convert.ToInt32(macDecByte[10])) * 16 + AscIIToInt((char)Convert.ToInt32(macDecByte[11])) * 1;//=> x的位       o o o o o o o o x x x x x x x x
                    int total = left + right;//相加

                    //保留 10 bit =>  (最大整数4095 如右边x的部分)=> o o o o o o x x x x x x x x x x   
                    total = total >> 6;//

                    return total;
                }
            }

            /// <summary>
            /// 将AscII码 转为 Int
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            private static int AscIIToInt(char item)
            {
                int resultValue = 0;
                //取得对应 Char -> Value
                dicAscii.TryGetValue(item, out resultValue);
                //返回16进制数值 
                return resultValue;
            }
        }
        #endregion

        #region TimeSpan Milliseconds 41bit
        /// <summary>
        /// 取得时间戳
        /// </summary>
        private static class GetTimeSpan
        {

            /// <summary>
            /// 回传当前时间微秒 Long型态
            /// </summary>
            /// <returns></returns>
            public static long GetTimeSpanReturnLong()
            {
                DateTime dt = DateTime.Now;//现在时间
                DateTime ori = new DateTime(1970, 1, 1, 0, 0, 0);//起源时间
                return (long)(dt - ori).TotalMilliseconds;
            }

            /// <summary>
            /// 回传当前时间微秒 + 1 Long 型态
            /// </summary>
            /// <returns></returns>
            public static long GetTimeSpanReturnNextSecondLong()
            {
                DateTime dt = DateTime.Now.AddMilliseconds(1);//增加1微秒
                DateTime ori = new DateTime(1970, 1, 1, 0, 0, 0);//起源时间
                return (long)(dt - ori).TotalMilliseconds;
            }

        }
        #endregion

        #region Sequence 12bit
        /// <summary>
        /// 取得序列号
        /// </summary>
        private static class GetSequence
        {
            /// <summary>
            /// 12bit 最大长度
            /// </summary>
            private static long BIT12 = 4095;

            public static bool checkSeq(long nowSeq)
            {
                var check = (nowSeq) & BIT12;
                if (check == 0)
                    return true;
                else
                    return false;
            }

        }

        #endregion

        #endregion
    }
}

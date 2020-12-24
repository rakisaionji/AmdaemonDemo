using AMDaemon;
using System;
using System.Threading;

namespace AmdaemonDemo
{
    class Program
    {
        private static AimeUnit aimeUnit = null;
        private static AimeResult aimeResult = null;

        private static bool Scan_Init()
        {
            return ((!aimeUnit.IsBusy || aimeUnit.Cancel()) && aimeUnit.Start(AimeCommand.Scan));
        }

        static bool Scan_Proc()
        {
            if (aimeUnit.IsBusy)
            {
                if (aimeUnit.HasConfirm)
                {
                    aimeUnit.SetLedStatus(AimeLedStatus.Warning);
                }
            }
            else if (aimeUnit.HasResult)
            {
                aimeResult = aimeUnit.Result;
                if (aimeResult != null && aimeResult.AimeId.IsValid)
                {
                    aimeUnit.SetLedStatus(AimeLedStatus.Success);
                    return true;
                }
                else
                {
                    aimeUnit.SetLedStatus(AimeLedStatus.Error);
                }
            }
            else if (aimeUnit.HasError)
            {
                aimeUnit.SetLedStatus(AimeLedStatus.Error);
            }
            return false;
        }
        static void Main(string[] args)
        {
            Core.Execute();

            Console.WriteLine($"MAIN ID  : {AMDaemon.System.BoardId}");
            Console.WriteLine($"KEYCHIP  : {AMDaemon.System.KeychipId}");
            Console.WriteLine();

            if (Aime.UnitCount <= 0 || Aime.IsFirmUpdating)
            {
                Console.WriteLine("Aime.UnitCount = 0");
                return;
            }
            aimeUnit = Aime.Units[0];

            Console.WriteLine($"HARDWARE : {aimeUnit.Result.HardVersion}");
            Console.WriteLine($"FIRMWARE : {aimeUnit.Result.FirmVersion}");
            Console.WriteLine();

            foreach (var campaignInfo in Aime.CampaignInfos)
            {
                var openTimeRange = campaignInfo.OpenTimeRange;
                var rewardTimeRange = campaignInfo.RewardTimeRange;
                Console.WriteLine("[CampaignInfo]");
                Console.WriteLine($"Id = {campaignInfo.Id}");
                Console.WriteLine($"Name = {campaignInfo.Name}");
                Console.WriteLine($"NoticeTime = {campaignInfo.NoticeTime}");
                Console.WriteLine($"OpenTimeRange = {openTimeRange.Begin} - {openTimeRange.End}");
                Console.WriteLine($"RewardTimeRange = {rewardTimeRange.Begin} - {rewardTimeRange.End}");
                Console.WriteLine();
            }

            while (true)
            {
                Scan_Init();
                Thread.Sleep(10000);
                aimeUnit.Cancel();

                if (Scan_Proc())
                {
                    Console.WriteLine("[AimeCard]");
                    Console.WriteLine($"AimeId = {aimeResult.AimeId}");
                    Console.WriteLine($"AccessCode = {aimeResult.AccessCode}");
                    Console.WriteLine($"OfflineId = {aimeResult.OfflineId}");
                    if (aimeResult.IsSegaIdRegistered)
                    {
                        Console.WriteLine($"SegaIdAuthKey = {aimeResult.SegaIdAuthKey}");
                    }
                    if (aimeResult.RelatedAimeIdCount > 0)
                    {
                        int i = 0;
                        foreach (var related in aimeResult.RelatedAimeIds)
                        {
                            Console.WriteLine($"RelatedAimeIds[{i++}] = {related.Value}");
                        }
                    }
                    Console.WriteLine();

                    var progresses = Aime.GetCampaignProgresses(aimeResult.AimeId);
                    foreach (var progress in progresses)
                    {
                        Console.WriteLine("[CampaignProgress]");
                        Console.WriteLine($"Id = {progress.Id}");
                        Console.WriteLine($"Bits = {progress.Bits}");
                        Console.WriteLine($"Entry = {progress.IsEntry}");
                        Console.WriteLine();
                    }
                }
            }
        }
    }
}

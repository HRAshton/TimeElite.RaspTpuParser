namespace RaspTpuIcalConverter.RaspTpuModels
{
    internal class RaspUrlModel
    {
        public string Id;
        public ushort Year;
        public byte Week;
        public string GetUrlForWeek(int delta) => $"https://rasp.tpu.ru/{Id}/{Year}/{Week + delta}/view.html";
    }
}
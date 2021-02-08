namespace Mikodev.Mines.Abstractions
{
    /// <summary>
    /// UI 标记
    /// </summary>
    public enum MineMark
    {
        /// <summary>
        /// 被翻开的方块 (数字 1 到 7 直接强制类型转换)
        /// </summary>
        None = 0,

        /// <summary>
        /// 未翻开的方块
        /// </summary>
        Tile = 0x80,

        /// <summary>
        /// 地雷
        /// </summary>
        Mine,

        /// <summary>
        /// 地雷 (错误点击)
        /// </summary>
        Trap,

        /// <summary>
        /// 旗帜
        /// </summary>
        Flag,

        /// <summary>
        /// 问号
        /// </summary>
        What,
    }
}

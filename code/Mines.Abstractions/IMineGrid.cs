namespace Mikodev.Mines.Abstractions
{
    /// <summary>
    /// 雷区
    /// </summary>
    public interface IMineGrid
    {
        /// <summary>
        /// 游戏结束
        /// </summary>
        bool IsGameOver { get; }

        /// <summary>
        /// X 最大值
        /// </summary>
        int XMax { get; }

        /// <summary>
        /// Y 最大值
        /// </summary>
        int YMax { get; }

        /// <summary>
        /// 获取指定坐标的内容
        /// </summary>
        MineMark Get(int x, int y);

        /// <summary>
        /// 设置标记 (旗帜, 问号, 或清除标记)
        /// </summary>
        void Set(int x, int y);

        /// <summary>
        /// 翻开方块
        /// </summary>
        void Remove(int x, int y);

        /// <summary>
        /// 根据旗帜翻开周围方块
        /// </summary>
        void RemoveAll(int x, int y);
    }
}

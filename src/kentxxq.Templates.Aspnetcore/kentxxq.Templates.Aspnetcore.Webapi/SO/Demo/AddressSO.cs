#if (EnableDB)
namespace kentxxq.Templates.Aspnetcore.Webapi.SO.Demo
{
    /// <summary>
    /// 用户地址信息
    /// </summary>
    public class AddressSO
    {
        /// <summary>
        /// 地址
        /// </summary>
        public string UserAddress { get; set; } = null!;

        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// 手机号
        /// </summary>
        public string Phone { get; set; } = null!;
    }
}
#endif
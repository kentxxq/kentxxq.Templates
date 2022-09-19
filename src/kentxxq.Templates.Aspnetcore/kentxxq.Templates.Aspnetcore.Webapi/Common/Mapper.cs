﻿using kentxxq.Templates.Aspnetcore.Webapi.Services.ExternalApi;
using kentxxq.Templates.Aspnetcore.Webapi.Services.Tools;
using kentxxq.Templates.Aspnetcore.Webapi.SO.Tools;
using Riok.Mapperly.Abstractions;

namespace kentxxq.Templates.Aspnetcore.Webapi.Common
{
    /// <summary>
    /// mapper转换工具类
    /// </summary>
    [Mapper]
    public static partial class Mapper
    {
        /// <summary>
        /// ipapi转换成ip通用模型
        /// </summary>
        /// <param name="ipApiModel"></param>
        /// <returns></returns>
        public static partial IpServiceModel IpApiModelToIpServiceModel(IpApiModel ipApiModel);

        /// <summary>
        /// ip通用模型转换成SO模型
        /// </summary>
        /// <param name="ipServiceModel"></param>
        /// <returns></returns>
        public static partial IpSO IpServiceModelToIpSO(IpServiceModel ipServiceModel);
    }
}
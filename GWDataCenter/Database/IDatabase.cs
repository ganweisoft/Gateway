﻿﻿// Copyright (c) 2004-2025 Shenzhen Ganwei Software Technology Co., Ltd
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace GWDataCenter.Database
{
    interface IDatabase
    {
        ServiceProvider Initialize<T>(string csPWD) where T : DbContext;
    }
}

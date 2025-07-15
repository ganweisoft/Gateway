﻿// Copyright (c) 2004-2025 Shenzhen Ganwei Software Technology Co., Ltd
namespace GWDataCenter
{
    public interface ISetItem
    {
        bool DoSetItem();
        bool m_bTemporarilyBreak
        {
            get;
            set;
        }
    }
}

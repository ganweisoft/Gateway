﻿// Copyright (c) 2004 Shenzhen Ganwei Software Technology Co., Ltd
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

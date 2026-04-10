Flow.Launcher.Plugin.BankCardFaker
==================

A plugin for the [Flow launcher](https://github.com/Flow-Launcher/Flow.Launcher).

## 生成国内虚拟银行卡号

- Flow Launcher 插件
- 国内银行卡卡号虚拟生成工具
- Chinese Bank Card Number Fake Generator
- 开发语言: C#
- 银行卡生成库, 参考: (https://github.com/DDU1222/bankcard-validator)

### Usage

#### 基础用法

```shell
fbn <arguments>
```

#### 按银行过滤

```shell
fbn 工商
## 内置银行简写
fbn icbc
## 简单首字符
fbn jtyh   # 交通银行
```

#### 指定银行卡号开始编码

```shell
# 将生成以 62 为开始的卡号
fbn 62
# 将生成以 44 为开始的卡号
fbn 44
```

#### 组合条件生成

```shell
# 生成 工商银行 信用卡, 卡号以 48 开头
fbn icbc xyk 48
fbn 工商银行 信用卡 48
fbn icbc xyk 48
```

### 内置银行简写

- 中国银行: boc
- 工商银行: icbc
- 农业银行: abc
- 建设银行: ccb
- 招商银行: cmb


# Measure CPU power tool

Measure cpu package power consumption.

# Install

```
dotnet tool update DeviceTool.CpuPower -g
```

# Usage

Run as Administrator.

```
PS D:\> cpupower
AMD Ryzen 9 5900X
Avg: 61.22
Min: 56.49
Max: 72.67
```

# CPU power consumption in idle

| CPU                                    | Min   | Max   | Avg   |
|:---------------------------------------|------:|------:|------:|
| AMD Ryzen 9 5900X                      | 56.49 | 72.67 | 61.22 |
| 12th Gen Intel Core i5-12500           | 18.98 | 22.11 | 20.08 |
| AMD Ryzen 7 7730U with Radeon Graphics |  1.90 |  3.36 |  2.42 |
| Intel Core i7-10710U                   |  5.01 |  9.07 |  6.00 |

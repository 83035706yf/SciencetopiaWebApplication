# Sciencetopia 后端

## 概述
Sciencetopia的后端是一个基于.NET 8 Web API的服务，它与Neo4j图数据库进行交互，为前端应用提供数据支持和业务逻辑处理。

## 主要特性
- **用户认证**：支持用户注册、登录和注销。
- **知识图谱管理**：提供知识图谱节点的获取和搜索功能。
- **学习计划**：允许用户创建和保存个性化学习计划。
- **收藏功能**：用户可以收藏他们感兴趣的知识节点。
- **推荐系统**：基于用户活动提供个性化推荐。

## 技术栈
- **.NET 8 Web API**：用于构建RESTful API服务。
- **Neo4j**：图数据库，用于存储和查询复杂的知识图谱数据。
- **身份验证**：使用Cookie认证机制。
- **CORS**：跨源资源共享支持。

---

## 如何开始

### **1. 克隆仓库**
运行以下命令克隆仓库到本地：
```bash
git clone https://github.com/Sciencetopia-org/SciencetopiaWebApplication.git
```

进入项目目录：
```bash
cd SciencetopiaWebApplication
```

---

### **2. 安装依赖**
确保已安装 [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)。

恢复项目依赖：
```bash
dotnet restore
```

---

### **3. 配置数据库**
在项目根目录中添加包含数据库连接字符串的 `appsettings.json` 文件。

---

### **4. 启动项目**
运行以下命令以开发模式启动项目：
```bash
dotnet run
```

默认情况下，项目将在 `http://localhost:5000` 或 `http://localhost:8080` 运行。

如果需要指定端口，可以运行：
```bash
dotnet run --urls "http://localhost:5001"
```

---

### **5. 验证本地运行**
1. 打开浏览器访问 `http://localhost:5000` 或您指定的端口。
2. 验证页面是否加载成功，确保功能正常运行。

---

## API文档
- 本地开发时，可通过 Swagger 查看 API 文档：
  - 访问 `http://localhost:5000/swagger` 或指定的端口 `/swagger`。

---

### **常见问题**
#### **问题 1：依赖项未正确安装**
运行以下命令恢复依赖：
```bash
dotnet restore
```

#### **问题 2：端口冲突**
如果运行项目时端口已被占用，请修改运行端口：
```bash
dotnet run --urls "http://localhost:5001"
```

#### **问题 3：无法访问页面**
检查终端输出是否有错误日志。如果有异常，请根据提示解决配置问题（例如 `appsettings.json` 中的 Neo4j 配置）。

---

### **注意事项**
- **始终拉取最新代码：** 在开发新功能之前，请确保同步代码库：
  ```bash
  git pull origin main
  ```

- **不要提交生成的编译文件：** 确保 `.gitignore` 文件中包含以下内容：
  ```plaintext
  bin/
  obj/
  ```

- **修改依赖时通知团队：** 如果您更新或新增了项目依赖，请告知团队成员运行以下命令：
  ```bash
  dotnet restore
  ```

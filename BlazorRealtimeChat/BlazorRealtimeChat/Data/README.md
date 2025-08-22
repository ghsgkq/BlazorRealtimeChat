# Blazor-Realtime-Chat: 데이터베이스 마이그레이션 가이드

이 문서는 Entity Framework Core(EF Core)를 사용하여 PostgreSQL 데이터베이스의 스키마를 관리하는 방법을 안내합니다. C#으로 작성된 엔티티(Entity) 클래스를 실제 데이터베이스 테이블로 만들거나 변경할 때 이 절차를 따릅니다.

## 📌 핵심 개념

- **엔티티(Entity):** C# 코드(`Data` 폴더의 클래스)로 정의된 데이터베이스 테이블의 설계도입니다.
- **마이그레이션(Migration):** 현재 코드의 엔티티와 실제 데이터베이스의 상태를 비교하여, 데이터베이스를 최신 상태로 변경하기 위한 **변경 내역 파일(설계도)**을 만드는 과정입니다.
- **데이터베이스 업데이트(Database Update):** 생성된 마이그레이션 파일을 기반으로 실제 데이터베이스에 `CREATE TABLE`, `ALTER COLUMN` 등의 SQL 명령을 실행하여 테이블 구조를 변경하는 과정입니다.

## 🚀 마이그레이션 실행 절차

### 1. 엔티티 클래스 변경

데이터베이스 구조를 변경하고 싶을 때, 가장 먼저 `Data` 폴더에 있는 C# 엔티티 클래스 파일을 수정합니다.

- 새 테이블 추가: 새로운 엔티티 클래스를 만들고 `DbContext`에 `DbSet`으로 등록합니다.
- 컬럼 추가/수정: 기존 엔티티 클래스에 속성(Property)을 추가하거나 타입을 변경합니다.

**예시:** `Product` 엔티티의 기본 키를 `int`에서 `Guid`로 변경

```
// 변경 전
public class Product
{
    [Key]
    public int ProductId { get; set; }
}

// 변경 후
public class Product
{
    [Key]
    public Guid ProductId { get; set; } // int -> Guid로 변경
}

```

### 2. 마이그레이션 파일 생성 (`dotnet ef migrations add`)

엔티티 코드 수정이 완료되면, 터미널(Terminal)에서 아래 명령어를 실행하여 변경 내역을 담은 마이그레이션 파일을 생성합니다.

```
dotnet ef migrations add <MigrationName>
```<MigrationName>` 부분에는 **어떤 변경을 했는지 알아보기 쉬운 이름**을 영어로 적어주는 것이 좋습니다.

**예시:**
```bash
# Product 키를 Guid로 변경한 내역을 추가
dotnet ef migrations add UseGuidForProductKey

```

이 명령어가 성공하면, 프로젝트에 `Migrations` 폴더가 생기고 그 안에 방금 생성한 마이그레이션 파일이 들어있는 것을 확인할 수 있습니다.

### 3. 데이터베이스에 적용 (`dotnet ef database update`)

마지막으로, 생성된 마이그레이션 파일을 실제 데이터베이스에 적용하여 테이블 구조를 변경합니다.

터미널에서 아래 명령어를 실행합니다.

```
dotnet ef database update

```

이 명령어가 성공적으로 완료되면 PostgreSQL 데이터베이스에 접속하여 테이블이 의도한 대로 생성되거나 변경되었는지 확인합니다.

## 🚨 문제 해결 (Troubleshooting)

- **`dotnet-ef` 명령어를 찾을 수 없다는 에러가 발생할 경우:**
    - EF Core 도구가 설치되지 않은 것입니다. 아래 명령어로 전역 도구를 설치하세요.
    - `dotnet tool install --global dotnet-ef`
- **`DbContext`를 만들 수 없다는 에러가 발생할 경우:**
    - `IDesignTimeDbContextFactory` 구현이 필요합니다. `DbContext`가 있는 폴더에 팩토리 클래스를 추가하여 문제를 해결할 수 있습니다.
- **`database update` 실행 시 연결 에러가 발생할 경우:**
    - `appsettings.json`의 연결 문자열(Connection String)이 정확한지 확인하세요.
    - PostgreSQL 서버가 실행 중인지 확인하세요.
    - 명령어를 실행하기 전에 데이터베이스가 **미리 생성**되어 있는지 확인하세요. 이 명령어는 데이터베이스를 만들어주지 않습니다.
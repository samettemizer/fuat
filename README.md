# FuatAmca — Apartment / Residential Complex Management

This application began at a management meeting for the four-building site where I live:<br>
“Samet, we need an ‘Excel program’; all this paperwork is getting difficult.”

FuatAmca provides a **REST API + CMS** for apartment building managers.<br>

**FuatAmca backend**: Uses .NET 9 API, EF Core and PostgreSQL<br>
**FuatAmca frontend**: Vue.js (will be add)<br>
**Authentication**: JWT Bearer<br>
**API docs**: Scalar

note: it was developed within a circle of friends, the service URLs contain Turkish expressions.

---

## Run

```bash
# Configure the passwords and port/origin settings in .env.
docker compose up -d --build
```


Initial login (using the values configured in `.env`):

```bash
curl -s http://localhost:8080/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"email":"yonetici@fuat.local","password":"FuatAmca.Parola.2026"}'
```


---

## API Overview

All endpoints require `Authorization: Bearer <token>`. Legacy paths are not retained as aliases.

### Authentication
| Method | Path | Description |
|---|---|---|
| POST | `/auth/login` | `{email,password}` → access/refresh tokens, roles, apartment buildings |
| POST | `/auth/refresh-token` | `{refreshToken}` → token rotation; reuse revokes all refresh sessions |
| POST | `/auth/logout` | `{refreshToken}` → number of revoked sessions |
| GET | `/auth/me` | Profile + roles + authorized apartment buildings |

### Panel Users
`GET|POST /panel-users`, `PUT|DELETE /panel-users/{userId}` (DELETE deactivates the user)<br>
`POST /panel-users/{userId}/reset-password`, `GET /roles`<br>
The last active administrator is protected. Users cannot deactivate their own accounts or delegate access to an apartment building they are not authorized to manage.

### Apartment Buildings
`POST /apartmanlar` (the creator is automatically granted access, and four expense categories are set up),<br>
`GET /apartmanlar` (member count, total debt/credit — the “Selected Apartment Building” list in the panel),<br>
`GET|PUT /apartmanlar/{apartmanId}`,<br>
`GET|PUT /apartmanlar/{apartmanId}/gider-ayarlari`

### Members
`GET|POST /members`<br>
`GET|PUT|DELETE /members/{memberId}`<br>
`GET` /members/export?format=xlsx|pdf.
The `create`/`update` request body includes `apartmanId`.<br>
`list/detail/delete/export` require the `apartmanId` query parameter.

### Financial Transactions
`GET|POST /para-hareketleri`,<br>
`DELETE /para-hareketleri/{hareketId}`,<br>
`POST /para-hareketleri/tahakkuk` (monthly expense assessment, idempotent),<br>
`GET /para-hareketleri/gecikmis-muhletler`.

Monthly expense categories: **Maintenance Fees, Fuel, Building Renovation, Miscellaneous Expenses**.<br>
- If the building uses individual combi boilers for heating, the fuel charge is **0 TL** for every member.<br>
- Building renovation costs **are not charged to tenants**.<br>
- Whenever a transaction is recorded, the member’s **master balance tables** (`MemberBakiyeleri`, `MemberBakiyeKalemleri`) are updated within the same transaction;
  list views read this summary instead of summing individual transactions.

---


## Roles and Permissions

| Role | Permissions |
|---|---|
| `Admin` (Administrator) | Apartment building creation/settings, panel users, members, financial transactions, announcements |
| `AssistantAdmin` (Assistant Administrator) | Member records and announcements + read access |
| `Treasurer` | Financial transactions, expense assessment + read access |
| `Auditor` | Read-only access |

Note: If there is **only one apartment building** in the system, a user with the `Admin` role is automatically granted access to that building **at login**.

## License

This project is licensed under the GNU Affero General Public License v3.0 only (AGPL-3.0-only). See [LICENSE](LICENSE) for the full license text.
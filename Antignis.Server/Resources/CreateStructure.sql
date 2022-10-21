CREATE TABLE "WindowsFirewallSetting" (
	"Id"	INTEGER NOT NULL UNIQUE,
	"PrivateProfileEnabled"	INTEGER,
	"PublicProfileEnabled"	INTEGER,
	"DomainProfileEnabled"	INTEGER,
	"PrivateProfileDefaultBlockAction"	TEXT,
	"PublicProfileDefaultBlockAction"	TEXT,
	"DomainProfileDefaultBlockAction"	TEXT,
	PRIMARY KEY("Id" AUTOINCREMENT)
);

CREATE TABLE "Host" (
	"Id"	INTEGER NOT NULL UNIQUE,
	"IsServerOS"	INTEGER,
	"OperatingSystem"	TEXT,
	"DNSHostname"	TEXT,
	"NetworkMask"	TEXT,
	"IPAddress"	TEXT,
	"WindowsFirewallSettingId"	INTEGER,
	PRIMARY KEY("Id" AUTOINCREMENT)
);
CREATE TABLE "FileShare" (
	"Id"	INTEGER NOT NULL UNIQUE,
	"Name"	TEXT,
	"HostId"	INTEGER,
	FOREIGN KEY("HostId") REFERENCES "Host"("Id"),
	PRIMARY KEY("Id" AUTOINCREMENT)
);

CREATE TABLE "Port" (
	"Id"	INTEGER NOT NULL UNIQUE,
	"HostId"	INTEGER,
	"PortNumber"	INTEGER,
	FOREIGN KEY("HostId") REFERENCES "Host"("Id"),
	PRIMARY KEY("Id" AUTOINCREMENT)
);

CREATE TABLE "Program"
(
	"Id" INTEGER NOT NULL UNIQUE,
	"HostId" INTEGER NULL,
	"Name" TEXT NULL,
	PRIMARY KEY("Id" AUTOINCREMENT),
	FOREIGN KEY("HostId") REFERENCES "Host"("Id")
);

CREATE TABLE "Role"
(
	"Id" INTEGER NOT NULL UNIQUE,
	"HostId" INTEGER NULL,
	"Name" TEXT NULL,
	PRIMARY KEY("Id" AUTOINCREMENT),
	FOREIGN KEY("HostId") REFERENCES "Host"("Id")
);

CREATE TABLE "TCPConnection" (
	"Id"	INTEGER NOT NULL UNIQUE,
	"HostId"	INTEGER,
	"Direction"	TEXT,
	"LocalIPAddress"	TEXT,
	"RemoteIPAddress"	TEXT,
	"LocalPort"	INTEGER,
	"RemotePort"	INTEGER,
	FOREIGN KEY("HostId") REFERENCES "Host"("Id"),
	PRIMARY KEY("Id" AUTOINCREMENT)
);

CREATE TABLE "WindowsFirewallRule" (
	"Id"	INTEGER NOT NULL UNIQUE,
	"HostId"	INTEGER,
	"LocalPorts"	TEXT,
	"RemoteAddresses"	TEXT,
	"Action"	TEXT,
	"RuleEnabled"	INTEGER,
	"Interfaces"	TEXT,
	"Profiles"	TEXT,
	"Name"	TEXT,
	FOREIGN KEY("HostId") REFERENCES "Host"("Id"),
	PRIMARY KEY("Id" AUTOINCREMENT)
);

CREATE TABLE "Query" (
	"Id"	INTEGER NOT NULL UNIQUE,
	QueryString	TEXT,
	"Name"	TEXT,
	PRIMARY KEY("Id" AUTOINCREMENT)
);

/*INSERT prebuilt queries*/
INSERT INTO "main"."Query" ("QueryString", "Name") VALUES ('SELECT DNSHostname, IPAddress, OperatingSystem 
FROM Host 
WHERE IsServerOS = 1;', 'List all servers');
INSERT INTO "main"."Query" ("QueryString", "Name") VALUES ('SELECT DNSHostname, IPAddress, OperatingSystem 
FROM Host 
WHERE IsServerOS = 0;', 'List all workstations');
INSERT INTO "main"."Query" ("QueryString", "Name") VALUES ('SELECT Host.DNSHostname, Host.IPAddress, FileShare.Name 
FROM Host, FileShare 
WHERE FileShare.HostId = Host.Id;', 'List all shares');
INSERT INTO "main"."Query" ("QueryString", "Name") VALUES ('Select Name FROM Role;', 'List all roles');
INSERT INTO "main"."Query" ("QueryString", "Name") VALUES ('SELECT Host.DNSHostname, Host.IPAddress, FileShare.Name 
--
FROM Host, FileShare, WindowsFirewallRule 
--
WHERE FileShare.HostId = Host.Id 
	AND WindowsFirewallRule.HostId = Host.id 
	AND WindowsFirewallRule.RuleEnabled = 1 
	AND WindowsFirewallRule.LocalPorts = "445";', 'List hosts with fileshares exposed and an SMB allow rule in Firewall');

INSERT INTO "main"."Query" ("QueryString", "Name") VALUES ('SELECT Host.DNSHostname, Host.IPAddress 
--
FROM Host, Role 
--
WHERE Host.IsServerOS = 1 
	AND Role.HostId = Host.Id 
	AND Role.Name = ''File and Storage Services'' 
	AND Host.Id NOT IN 
		(SELECT HostId FROM FileShare);', 'List servers with Fileserver role having no fileshares exposed');

INSERT INTO "main"."Query" ("QueryString", "Name") VALUES ('SELECT Host.DNSHostname, Host.IPAddress, FileShare.Name 
--
FROM Host, FileShare 
--
WHERE Host.IsServerOS = 0 
	AND FileShare.HostId = Host.Id;', 'List workstations with fileshares exposed');

INSERT INTO "main"."Query" ("QueryString", "Name") VALUES ('SELECT DISTINCT	s.DNSHostname AS SourceComputer,
		d.DNSHostname AS DestinationComputer,
		t.RemotePort AS ConnectedToPort
--
FROM Host AS s
--
INNER JOIN TCPConnection t ON t.HostId = s.Id
INNER JOIN Host d ON d.IPAddress = t.RemoteIPAddress
--
WHERE t.Direction = "Outbound";', 'List outbound established TCP connections between hosts');

INSERT INTO "main"."Query" ("QueryString", "Name") VALUES ('SELECT DISTINCT DNSHostname FROM Host h
--
INNER JOIN Role r ON r.HostId = h.Id 
--
WHERE r.Name IN 
	("Remote Desktop Session Host", 
	"Remote Desktop Connection Broker", 
	"Remote Desktop Web Access");', 'List hosts with RDP role installed');

INSERT INTO "main"."Query" ("QueryString", "Name") VALUES ('SELECT DISTINCT DNSHostname FROM HOST
WHERE DNSHostName NOT IN (
    SELECT DISTINCT DNSHostName FROM Host h
    INNER JOIN Role r ON r.HostId = h.Id 
    WHERE r.Name IN (
		"Remote Desktop Session Host", 
		"Remote Desktop Connection Broker", 
		"Remote Desktop Web Access"
	)
);', 'List hosts with no RDP role installed');

INSERT INTO "main"."Query" ("QueryString", "Name") VALUES ('SELECT h.DNSHostname, wfs.PrivateProfileDefaultBlockAction, wfs.DomainProfileDefaultBlockAction, wfs.PublicProfileDefaultBlockAction
-- 
FROM WindowsFirewallSetting wfs
--
INNER JOIN Host h ON h.WindowsFirewallSettingId = wfs.Id
--
WHERE wfs.PrivateProfileDefaultBlockAction != "NET_FW_ACTION_BLOCK"
	OR wfs.DomainProfileDefaultBlockAction != "NET_FW_ACTION_BLOCK"
	OR wfs.PublicProfileDefaultBlockAction != "NET_FW_ACTION_BLOCK";', 'List hosts where inbound traffic is accepted by default');

CREATE INDEX "IX_FK_FileShare_HostId" ON "FileShare" (
	"HostId"	ASC
);

CREATE INDEX "IX_FK_Host_WindowsFirewallSettingsID" ON "Host" (
	"WindowsFirewallSettingId"	ASC
);

CREATE INDEX "IX_FK_Port_HostId" ON "Port" (
	"HostId"	ASC
);

CREATE INDEX "IX_FK_Program_HostId" ON "Program" (
	"HostId"	ASC
);

CREATE INDEX "IX_FK_Role_HostId" ON "Role" (
	"HostId"	ASC
);

CREATE INDEX "IX_FK_TCPConnection_HostId" ON "TCPConnection" (
	"HostId"	ASC
);

CREATE INDEX "IX_FK_WindowsFirewallRule_HostId" ON "WindowsFirewallRule" (
	"HostId"	ASC
);
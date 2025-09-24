import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "./ui/card";
import { Button } from "./ui/button";
import { Badge } from "./ui/badge";
import { Input } from "./ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "./ui/select";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "./ui/tabs";
import { Switch } from "./ui/switch";
import { Textarea } from "./ui/textarea";
import { 
  Users, 
  Settings, 
  Shield, 
  Database,
  User,
  Mail,
  Phone,
  Calendar,
  Edit,
  Trash2,
  Plus,
  Save,
  Download,
  Upload,
  RefreshCw,
  Bell,
  Lock,
  Globe,
  Monitor,
  AlertCircle,
  CheckCircle2
} from "lucide-react";

interface User {
  id: string;
  username: string;
  email: string;
  fullName: string;
  role: "manager" | "supervisor" | "planner" | "operator" | "driver" | "basic";
  department: string;
  status: "active" | "inactive" | "suspended";
  lastLogin: string;
  createdDate: string;
  phone?: string;
  permissions: string[];
}

interface SystemSetting {
  id: string;
  category: "general" | "security" | "notifications" | "integration";
  name: string;
  description: string;
  type: "boolean" | "string" | "number" | "select";
  value: any;
  options?: string[];
  requiresRestart?: boolean;
}

const roles = ["manager", "supervisor", "planner", "operator", "driver", "basic"];
const departments = ["Production", "Shipping", "Quality", "Maintenance", "Administration", "Sales"];

const sampleUsers: User[] = [
  {
    id: "USR-001",
    username: "s.chen",
    email: "sarah.chen@metalflow.com",
    fullName: "Sarah Chen",
    role: "manager",
    department: "Production",
    status: "active",
    lastLogin: "2024-01-15T08:30:00Z",
    createdDate: "2020-03-15",
    phone: "(555) 123-4567",
    permissions: ["all"]
  },
  {
    id: "USR-002",
    username: "j.miller",
    email: "john.miller@metalflow.com", 
    fullName: "John Miller",
    role: "supervisor",
    department: "Production",
    status: "active",
    lastLogin: "2024-01-15T07:45:00Z",
    createdDate: "2020-08-22",
    phone: "(555) 987-6543",
    permissions: ["production.read", "production.write", "inventory.read", "scheduling.read", "scheduling.write"]
  },
  {
    id: "USR-003",
    username: "m.rodriguez", 
    email: "mike.rodriguez@metalflow.com",
    fullName: "Mike Rodriguez",
    role: "operator",
    department: "Production",
    status: "active", 
    lastLogin: "2024-01-15T06:00:00Z",
    createdDate: "2021-02-10",
    phone: "(555) 555-0123",
    permissions: ["production.read", "machines.operate", "workorders.read"]
  },
  {
    id: "USR-004",
    username: "m.santos",
    email: "maria.santos@metalflow.com",
    fullName: "Maria Santos", 
    role: "operator",
    department: "Shipping",
    status: "active",
    lastLogin: "2024-01-14T16:30:00Z",
    createdDate: "2021-07-15",
    phone: "(555) 321-6789",
    permissions: ["shipping.read", "shipping.write", "inventory.read"]
  },
  {
    id: "USR-005",
    username: "c.martinez",
    email: "carlos.martinez@metalflow.com",
    fullName: "Carlos Martinez",
    role: "driver",
    department: "Shipping", 
    status: "active",
    lastLogin: "2024-01-15T05:30:00Z",
    createdDate: "2022-01-20",
    phone: "(555) 654-3210",
    permissions: ["mobile.access", "deliveries.read", "deliveries.write"]
  }
];

const systemSettings: SystemSetting[] = [
  {
    id: "SET-001",
    category: "general",
    name: "Company Name",
    description: "Name displayed throughout the application",
    type: "string",
    value: "MetalFlow WMS"
  },
  {
    id: "SET-002", 
    category: "general",
    name: "Timezone",
    description: "Default timezone for all date/time displays",
    type: "select",
    value: "America/Chicago",
    options: ["America/New_York", "America/Chicago", "America/Denver", "America/Los_Angeles"]
  },
  {
    id: "SET-003",
    category: "general",
    name: "Auto-refresh Interval",
    description: "How often dashboards refresh automatically (seconds)",
    type: "number",
    value: 30
  },
  {
    id: "SET-004",
    category: "security",
    name: "Session Timeout",
    description: "User session timeout in minutes",
    type: "number", 
    value: 480,
    requiresRestart: true
  },
  {
    id: "SET-005",
    category: "security",
    name: "Require Strong Passwords",
    description: "Enforce password complexity requirements",
    type: "boolean",
    value: true
  },
  {
    id: "SET-006",
    category: "security", 
    name: "Enable Two-Factor Authentication",
    description: "Require 2FA for all users",
    type: "boolean",
    value: false,
    requiresRestart: true
  },
  {
    id: "SET-007",
    category: "notifications",
    name: "Email Notifications",
    description: "Send email notifications for alerts and updates",
    type: "boolean",
    value: true
  },
  {
    id: "SET-008",
    category: "notifications",
    name: "SMS Notifications",
    description: "Send SMS notifications for critical alerts",
    type: "boolean",
    value: false
  },
  {
    id: "SET-009",
    category: "integration",
    name: "ERP Integration",
    description: "Enable automatic ERP data synchronization",
    type: "boolean", 
    value: true
  },
  {
    id: "SET-010",
    category: "integration",
    name: "ERP Sync Interval",
    description: "How often to sync with ERP system (minutes)",
    type: "number",
    value: 15
  }
];

const getRoleColor = (role: string) => {
  switch (role) {
    case "manager": return "bg-purple-100 text-purple-800 border-purple-200";
    case "supervisor": return "bg-blue-100 text-blue-800 border-blue-200";
    case "planner": return "bg-teal-100 text-teal-800 border-teal-200";
    case "operator": return "bg-green-100 text-green-800 border-green-200";
    case "driver": return "bg-orange-100 text-orange-800 border-orange-200";
    case "basic": return "bg-gray-100 text-gray-800 border-gray-200";
    default: return "bg-gray-100 text-gray-800 border-gray-200";
  }
};

const getStatusColor = (status: string) => {
  switch (status) {
    case "active": return "bg-green-100 text-green-800 border-green-200";
    case "inactive": return "bg-gray-100 text-gray-800 border-gray-200";
    case "suspended": return "bg-red-100 text-red-800 border-red-200";
    default: return "bg-gray-100 text-gray-800 border-gray-200";
  }
};

export function Admin() {
  const [activeTab, setActiveTab] = useState("users");
  const [selectedUser, setSelectedUser] = useState<User | null>(null);
  const [isEditingUser, setIsEditingUser] = useState(false);
  const [settings, setSettings] = useState(systemSettings);

  const updateSetting = (settingId: string, value: any) => {
    setSettings(prev => prev.map(setting => 
      setting.id === settingId ? { ...setting, value } : setting
    ));
  };

  return (
    <div className="space-y-6">
      <div>
        <h1>System Administration</h1>
        <p className="text-muted-foreground">System settings, users, and configuration</p>
      </div>

      <Tabs value={activeTab} onValueChange={setActiveTab} className="space-y-6">
        <TabsList className="grid w-full grid-cols-4">
          <TabsTrigger value="users" className="flex items-center gap-2">
            <Users className="h-4 w-4" />
            User Management
          </TabsTrigger>
          <TabsTrigger value="settings" className="flex items-center gap-2">
            <Settings className="h-4 w-4" />
            System Settings
          </TabsTrigger>
          <TabsTrigger value="security" className="flex items-center gap-2">
            <Shield className="h-4 w-4" />
            Security
          </TabsTrigger>
          <TabsTrigger value="database" className="flex items-center gap-2">
            <Database className="h-4 w-4" />
            Database
          </TabsTrigger>
        </TabsList>

        <TabsContent value="users" className="space-y-6">
          <div className="flex justify-between items-center">
            <div className="flex gap-2">
              <Input placeholder="Search users..." className="w-64" />
              <Select>
                <SelectTrigger className="w-40">
                  <SelectValue placeholder="Filter by role" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Roles</SelectItem>
                  {roles.map(role => (
                    <SelectItem key={role} value={role} className="capitalize">
                      {role}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <Button onClick={() => setIsEditingUser(true)}>
              <Plus className="h-4 w-4 mr-2" />
              New User
            </Button>
          </div>

          {/* User Statistics */}
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <Card>
              <CardContent className="pt-6">
                <div className="flex items-center">
                  <div className="p-2 bg-teal-100 rounded-full">
                    <Users className="h-4 w-4 text-teal-600" />
                  </div>
                  <div className="ml-4">
                    <p className="text-sm text-muted-foreground">Total Users</p>
                    <p className="text-2xl font-medium">{sampleUsers.length}</p>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardContent className="pt-6">
                <div className="flex items-center">
                  <div className="p-2 bg-green-100 rounded-full">
                    <CheckCircle2 className="h-4 w-4 text-green-600" />
                  </div>
                  <div className="ml-4">
                    <p className="text-sm text-muted-foreground">Active Users</p>
                    <p className="text-2xl font-medium">
                      {sampleUsers.filter(u => u.status === 'active').length}
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardContent className="pt-6">
                <div className="flex items-center">
                  <div className="p-2 bg-blue-100 rounded-full">
                    <Monitor className="h-4 w-4 text-blue-600" />
                  </div>
                  <div className="ml-4">
                    <p className="text-sm text-muted-foreground">Online Now</p>
                    <p className="text-2xl font-medium">3</p>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardContent className="pt-6">
                <div className="flex items-center">
                  <div className="p-2 bg-orange-100 rounded-full">
                    <AlertCircle className="h-4 w-4 text-orange-600" />
                  </div>
                  <div className="ml-4">
                    <p className="text-sm text-muted-foreground">Inactive</p>
                    <p className="text-2xl font-medium">
                      {sampleUsers.filter(u => u.status !== 'active').length}
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Users List */}
          <div className="space-y-4">
            {sampleUsers.map(user => (
              <Card key={user.id} className="hover:shadow-md transition-shadow">
                <CardHeader className="pb-3">
                  <div className="flex items-start justify-between">
                    <div>
                      <CardTitle className="text-lg flex items-center gap-2">
                        <User className="h-5 w-5" />
                        {user.fullName}
                      </CardTitle>
                      <div className="flex items-center gap-4 mt-2 text-sm text-muted-foreground">
                        <span>@{user.username}</span>
                        <span>•</span>
                        <span>{user.department}</span>
                        <span>•</span>
                        <span className="flex items-center gap-1">
                          <Calendar className="h-3 w-3" />
                          Joined {new Date(user.createdDate).getFullYear()}
                        </span>
                      </div>
                    </div>
                    <div className="flex items-center gap-2">
                      <Badge variant="outline" className={getRoleColor(user.role)}>
                        {user.role}
                      </Badge>
                      <Badge variant="outline" className={getStatusColor(user.status)}>
                        {user.status}
                      </Badge>
                    </div>
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="space-y-4">
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                      <div className="space-y-2">
                        <div className="flex items-center gap-2 text-sm">
                          <Mail className="h-4 w-4 text-muted-foreground" />
                          <span>{user.email}</span>
                        </div>
                        {user.phone && (
                          <div className="flex items-center gap-2 text-sm">
                            <Phone className="h-4 w-4 text-muted-foreground" />
                            <span>{user.phone}</span>
                          </div>
                        )}
                        <div className="text-sm">
                          <span className="text-muted-foreground">Last Login:</span>
                          <p className="font-medium">{new Date(user.lastLogin).toLocaleString()}</p>
                        </div>
                      </div>
                      
                      <div className="space-y-2">
                        <div className="text-sm">
                          <span className="text-muted-foreground">User ID:</span>
                          <p className="font-medium">{user.id}</p>
                        </div>
                        <div className="text-sm">
                          <span className="text-muted-foreground">Permissions:</span>
                          <p className="font-medium text-xs">
                            {user.permissions.includes('all') ? 'Full Access' : `${user.permissions.length} permissions`}
                          </p>
                        </div>
                      </div>
                    </div>

                    <div className="flex justify-end gap-2 pt-3 border-t">
                      <Button variant="outline" size="sm" onClick={() => setSelectedUser(user)}>
                        <Edit className="h-4 w-4 mr-2" />
                        Edit
                      </Button>
                      {user.status === 'active' ? (
                        <Button variant="outline" size="sm">
                          Suspend
                        </Button>
                      ) : (
                        <Button variant="outline" size="sm">
                          Activate
                        </Button>
                      )}
                      <Button variant="outline" size="sm" className="text-red-600 hover:text-red-700">
                        <Trash2 className="h-4 w-4 mr-2" />
                        Delete
                      </Button>
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </TabsContent>

        <TabsContent value="settings" className="space-y-6">
          <div className="flex justify-between items-center">
            <div>
              <h2 className="text-xl font-medium">System Settings</h2>
              <p className="text-sm text-muted-foreground">Configure application behavior and preferences</p>
            </div>
            <Button>
              <Save className="h-4 w-4 mr-2" />
              Save Changes
            </Button>
          </div>

          {/* Settings by Category */}
          {["general", "security", "notifications", "integration"].map(category => (
            <Card key={category}>
              <CardHeader>
                <CardTitle className="capitalize flex items-center gap-2">
                  {category === "general" && <Settings className="h-5 w-5" />}
                  {category === "security" && <Shield className="h-5 w-5" />}
                  {category === "notifications" && <Bell className="h-5 w-5" />}
                  {category === "integration" && <Globe className="h-5 w-5" />}
                  {category} Settings
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-6">
                  {settings.filter(setting => setting.category === category).map(setting => (
                    <div key={setting.id} className="flex items-center justify-between py-3 border-b last:border-b-0">
                      <div className="flex-1">
                        <div className="flex items-center gap-2">
                          <h4 className="font-medium">{setting.name}</h4>
                          {setting.requiresRestart && (
                            <Badge variant="outline" className="text-xs">
                              <RefreshCw className="h-3 w-3 mr-1" />
                              Restart Required
                            </Badge>
                          )}
                        </div>
                        <p className="text-sm text-muted-foreground mt-1">{setting.description}</p>
                      </div>
                      
                      <div className="flex-shrink-0 ml-4">
                        {setting.type === "boolean" && (
                          <Switch 
                            checked={setting.value}
                            onCheckedChange={(checked) => updateSetting(setting.id, checked)}
                          />
                        )}
                        {setting.type === "string" && (
                          <Input 
                            value={setting.value}
                            onChange={(e) => updateSetting(setting.id, e.target.value)}
                            className="w-48"
                          />
                        )}
                        {setting.type === "number" && (
                          <Input 
                            type="number"
                            value={setting.value}
                            onChange={(e) => updateSetting(setting.id, parseInt(e.target.value))}
                            className="w-24"
                          />
                        )}
                        {setting.type === "select" && setting.options && (
                          <Select value={setting.value} onValueChange={(value) => updateSetting(setting.id, value)}>
                            <SelectTrigger className="w-48">
                              <SelectValue />
                            </SelectTrigger>
                            <SelectContent>
                              {setting.options.map(option => (
                                <SelectItem key={option} value={option}>
                                  {option}
                                </SelectItem>
                              ))}
                            </SelectContent>
                          </Select>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          ))}
        </TabsContent>

        <TabsContent value="security" className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Lock className="h-5 w-5" />
                  Access Control
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="space-y-2">
                    <h4 className="font-medium">Role-Based Permissions</h4>
                    <p className="text-sm text-muted-foreground">
                      Manage what each role can access and modify
                    </p>
                    <Button variant="outline" className="w-full">
                      Configure Permissions
                    </Button>
                  </div>

                  <div className="space-y-2">
                    <h4 className="font-medium">IP Restrictions</h4>
                    <p className="text-sm text-muted-foreground">
                      Restrict access to specific IP addresses or ranges
                    </p>
                    <Textarea 
                      placeholder="Enter IP addresses or ranges..."
                      className="min-h-[80px]"
                    />
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Shield className="h-5 w-5" />
                  Security Monitoring
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="space-y-3">
                    <div className="flex items-center justify-between">
                      <span className="text-sm">Failed Login Attempts</span>
                      <Badge variant="secondary">0</Badge>
                    </div>
                    <div className="flex items-center justify-between">
                      <span className="text-sm">Suspicious Activities</span>
                      <Badge variant="secondary">0</Badge>
                    </div>
                    <div className="flex items-center justify-between">
                      <span className="text-sm">Active Sessions</span>
                      <Badge variant="secondary">3</Badge>
                    </div>
                  </div>

                  <Button variant="outline" className="w-full">
                    View Security Log
                  </Button>
                </div>
              </CardContent>
            </Card>
          </div>

          <Card>
            <CardHeader>
              <CardTitle>Security Policies</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <div className="space-y-4">
                    <div className="flex items-center justify-between">
                      <div>
                        <h4 className="font-medium">Password Complexity</h4>
                        <p className="text-sm text-muted-foreground">Require strong passwords</p>
                      </div>
                      <Switch defaultChecked />
                    </div>

                    <div className="flex items-center justify-between">
                      <div>
                        <h4 className="font-medium">Account Lockout</h4>
                        <p className="text-sm text-muted-foreground">Lock accounts after failed attempts</p>
                      </div>
                      <Switch defaultChecked />
                    </div>

                    <div className="flex items-center justify-between">
                      <div>
                        <h4 className="font-medium">Audit Logging</h4>
                        <p className="text-sm text-muted-foreground">Log all user activities</p>
                      </div>
                      <Switch defaultChecked />
                    </div>
                  </div>

                  <div className="space-y-4">
                    <div className="flex items-center justify-between">
                      <div>
                        <h4 className="font-medium">Session Management</h4>
                        <p className="text-sm text-muted-foreground">Automatic session timeout</p>
                      </div>
                      <Switch defaultChecked />
                    </div>

                    <div className="flex items-center justify-between">
                      <div>
                        <h4 className="font-medium">Data Encryption</h4>
                        <p className="text-sm text-muted-foreground">Encrypt sensitive data</p>
                      </div>
                      <Switch defaultChecked />
                    </div>

                    <div className="flex items-center justify-between">
                      <div>
                        <h4 className="font-medium">API Rate Limiting</h4>
                        <p className="text-sm text-muted-foreground">Limit API request rates</p>
                      </div>
                      <Switch defaultChecked />
                    </div>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="database" className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Database className="h-5 w-5" />
                  Database Status
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="grid grid-cols-2 gap-4 text-sm">
                    <div>
                      <span className="text-muted-foreground">Status:</span>
                      <p className="font-medium text-green-600">Connected</p>
                    </div>
                    <div>
                      <span className="text-muted-foreground">Database Size:</span>
                      <p className="font-medium">2.4 GB</p>
                    </div>
                    <div>
                      <span className="text-muted-foreground">Last Backup:</span>
                      <p className="font-medium">Jan 15, 2024 02:00</p>
                    </div>
                    <div>
                      <span className="text-muted-foreground">Next Backup:</span>
                      <p className="font-medium">Jan 16, 2024 02:00</p>
                    </div>
                  </div>

                  <div className="space-y-2">
                    <div className="flex justify-between text-sm">
                      <span>Storage Used</span>
                      <span>65%</span>
                    </div>
                    <div className="w-full bg-gray-200 rounded-full h-2">
                      <div className="bg-teal-600 h-2 rounded-full" style={{ width: "65%" }} />
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Database Operations</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-3">
                  <Button variant="outline" className="w-full justify-start">
                    <Download className="h-4 w-4 mr-2" />
                    Create Backup
                  </Button>
                  <Button variant="outline" className="w-full justify-start">
                    <Upload className="h-4 w-4 mr-2" />
                    Restore Backup
                  </Button>
                  <Button variant="outline" className="w-full justify-start">
                    <RefreshCw className="h-4 w-4 mr-2" />
                    Optimize Database
                  </Button>
                  <Button variant="outline" className="w-full justify-start">
                    <Database className="h-4 w-4 mr-2" />
                    View Connection Logs
                  </Button>
                </div>
              </CardContent>
            </Card>
          </div>

          <Card>
            <CardHeader>
              <CardTitle>Backup Configuration</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-6">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <div className="space-y-4">
                    <div className="flex items-center justify-between">
                      <div>
                        <h4 className="font-medium">Automatic Backups</h4>
                        <p className="text-sm text-muted-foreground">Schedule regular backups</p>
                      </div>
                      <Switch defaultChecked />
                    </div>

                    <div>
                      <label className="text-sm font-medium">Backup Frequency</label>
                      <Select defaultValue="daily">
                        <SelectTrigger>
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="hourly">Hourly</SelectItem>
                          <SelectItem value="daily">Daily</SelectItem>
                          <SelectItem value="weekly">Weekly</SelectItem>
                        </SelectContent>
                      </Select>
                    </div>
                  </div>

                  <div className="space-y-4">
                    <div>
                      <label className="text-sm font-medium">Retention Period</label>
                      <Select defaultValue="30">
                        <SelectTrigger>
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="7">7 days</SelectItem>
                          <SelectItem value="30">30 days</SelectItem>
                          <SelectItem value="90">90 days</SelectItem>
                          <SelectItem value="365">1 year</SelectItem>
                        </SelectContent>
                      </Select>
                    </div>

                    <div>
                      <label className="text-sm font-medium">Backup Location</label>
                      <Input defaultValue="/backups/metalflow" />
                    </div>
                  </div>
                </div>

                <div className="flex justify-end">
                  <Button>
                    <Save className="h-4 w-4 mr-2" />
                    Save Configuration
                  </Button>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
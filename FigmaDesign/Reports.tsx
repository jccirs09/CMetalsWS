import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "./ui/card";
import { Button } from "./ui/button";
import { Badge } from "./ui/badge";
import { Input } from "./ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "./ui/select";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "./ui/tabs";
import { Calendar } from "./ui/calendar";
import { Popover, PopoverContent, PopoverTrigger } from "./ui/popover";
import { 
  BarChart3, 
  TrendingUp, 
  FileText, 
  Download,
  Calendar as CalendarIcon,
  Filter,
  RefreshCw,
  Eye,
  Settings,
  Share,
  Clock,
  Package,
  Truck,
  Users,
  DollarSign,
  Activity
} from "lucide-react";
import { LineChart, Line, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, PieChart, Pie, Cell } from "recharts";

interface Report {
  id: string;
  name: string;
  description: string;
  category: "production" | "quality" | "shipping" | "inventory" | "financial" | "operational";
  type: "chart" | "table" | "dashboard";
  frequency: "real-time" | "daily" | "weekly" | "monthly";
  lastGenerated: string;
  format: "pdf" | "excel" | "csv";
  recipients: string[];
}

const sampleReports: Report[] = [
  {
    id: "RPT-001",
    name: "Production Efficiency",
    description: "Machine utilization and throughput analysis",
    category: "production",
    type: "dashboard",
    frequency: "real-time",
    lastGenerated: "2024-01-15T10:30:00Z",
    format: "pdf",
    recipients: ["production@metalflow.com", "management@metalflow.com"]
  },
  {
    id: "RPT-002",
    name: "Quality Metrics",
    description: "Defect rates and quality control statistics",
    category: "quality",
    type: "chart",
    frequency: "daily",
    lastGenerated: "2024-01-15T06:00:00Z",
    format: "pdf",
    recipients: ["quality@metalflow.com"]
  },
  {
    id: "RPT-003",
    name: "Shipping Performance",
    description: "On-time delivery rates and shipping costs",
    category: "shipping",
    type: "table",
    frequency: "weekly",
    lastGenerated: "2024-01-14T18:00:00Z",
    format: "excel",
    recipients: ["shipping@metalflow.com", "logistics@metalflow.com"]
  },
  {
    id: "RPT-004",
    name: "Inventory Turnover",
    description: "Stock levels and inventory movement analysis",
    category: "inventory",
    type: "chart",
    frequency: "monthly",
    lastGenerated: "2024-01-01T09:00:00Z",
    format: "pdf",
    recipients: ["inventory@metalflow.com", "procurement@metalflow.com"]
  }
];

// Sample data for charts
const productionData = [
  { date: "Jan 1", ctlLine1: 85, ctlLine2: 78, slitter1: 92, slitter2: 88 },
  { date: "Jan 2", ctlLine1: 92, ctlLine2: 85, slitter1: 89, slitter2: 91 },
  { date: "Jan 3", ctlLine1: 78, ctlLine2: 92, slitter1: 95, slitter2: 87 },
  { date: "Jan 4", ctlLine1: 89, ctlLine2: 88, slitter1: 88, slitter2: 93 },
  { date: "Jan 5", ctlLine1: 95, ctlLine2: 91, slitter1: 91, slitter2: 89 },
  { date: "Jan 6", ctlLine1: 87, ctlLine2: 89, slitter1: 87, slitter2: 92 },
  { date: "Jan 7", ctlLine1: 91, ctlLine2: 94, slitter1: 93, slitter2: 95 }
];

const qualityData = [
  { month: "Oct", defectRate: 2.1, customerComplaints: 5, rework: 3.2 },
  { month: "Nov", defectRate: 1.8, customerComplaints: 3, rework: 2.9 },
  { month: "Dec", defectRate: 1.5, customerComplaints: 2, rework: 2.1 },
  { month: "Jan", defectRate: 1.2, customerComplaints: 1, rework: 1.8 }
];

const shippingData = [
  { week: "Week 1", onTime: 94, late: 6, cost: 12500 },
  { week: "Week 2", onTime: 97, late: 3, cost: 11800 },
  { week: "Week 3", onTime: 91, late: 9, cost: 13200 },
  { week: "Week 4", onTime: 96, late: 4, cost: 12100 }
];

const inventoryData = [
  { name: "Cold Rolled Steel", value: 35, color: "#0d9488" },
  { name: "Stainless Steel", value: 25, color: "#14b8a6" },
  { name: "Galvanized Steel", value: 20, color: "#2dd4bf" },
  { name: "Aluminum", value: 15, color: "#5eead4" },
  { name: "Other", value: 5, color: "#99f6e4" }
];

const kpiData = [
  { name: "Production Output", value: "847,230 ft", change: "+12.5%", trend: "up" },
  { name: "Quality Score", value: "98.8%", change: "+2.1%", trend: "up" },
  { name: "On-Time Delivery", value: "94.5%", change: "-1.2%", trend: "down" },
  { name: "Inventory Turns", value: "8.4x", change: "+0.8x", trend: "up" },
  { name: "Cost Per Unit", value: "$2.14", change: "-$0.08", trend: "down" },
  { name: "Customer Satisfaction", value: "4.7/5", change: "+0.2", trend: "up" }
];

const getCategoryIcon = (category: string) => {
  switch (category) {
    case "production": return <Activity className="h-4 w-4" />;
    case "quality": return <Badge className="h-4 w-4" />;
    case "shipping": return <Truck className="h-4 w-4" />;
    case "inventory": return <Package className="h-4 w-4" />;
    case "financial": return <DollarSign className="h-4 w-4" />;
    case "operational": return <Settings className="h-4 w-4" />;
    default: return <FileText className="h-4 w-4" />;
  }
};

const getCategoryColor = (category: string) => {
  switch (category) {
    case "production": return "bg-teal-100 text-teal-800 border-teal-200";
    case "quality": return "bg-green-100 text-green-800 border-green-200";
    case "shipping": return "bg-blue-100 text-blue-800 border-blue-200";
    case "inventory": return "bg-purple-100 text-purple-800 border-purple-200";
    case "financial": return "bg-yellow-100 text-yellow-800 border-yellow-200";
    case "operational": return "bg-gray-100 text-gray-800 border-gray-200";
    default: return "bg-gray-100 text-gray-800 border-gray-200";
  }
};

export function Reports() {
  const [activeTab, setActiveTab] = useState("dashboard");
  const [selectedDateRange, setSelectedDateRange] = useState("7d");
  const [selectedCategory, setSelectedCategory] = useState("all");

  const filteredReports = sampleReports.filter(report => 
    selectedCategory === "all" || report.category === selectedCategory
  );

  return (
    <div className="space-y-6">
      <div>
        <h1>Reports & Analytics</h1>
        <p className="text-muted-foreground">Performance analytics and operational reports</p>
      </div>

      <Tabs value={activeTab} onValueChange={setActiveTab} className="space-y-6">
        <TabsList className="grid w-full grid-cols-4">
          <TabsTrigger value="dashboard" className="flex items-center gap-2">
            <BarChart3 className="h-4 w-4" />
            Analytics Dashboard
          </TabsTrigger>
          <TabsTrigger value="reports" className="flex items-center gap-2">
            <FileText className="h-4 w-4" />
            Reports
          </TabsTrigger>
          <TabsTrigger value="production" className="flex items-center gap-2">
            <Activity className="h-4 w-4" />
            Production
          </TabsTrigger>
          <TabsTrigger value="quality" className="flex items-center gap-2">
            <TrendingUp className="h-4 w-4" />
            Quality & KPIs
          </TabsTrigger>
        </TabsList>

        {/* Date Range Controls */}
        <div className="flex flex-wrap gap-4 items-center justify-between">
          <div className="flex gap-2 items-center">
            <Select value={selectedDateRange} onValueChange={setSelectedDateRange}>
              <SelectTrigger className="w-32">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="1d">Today</SelectItem>
                <SelectItem value="7d">Last 7 days</SelectItem>
                <SelectItem value="30d">Last 30 days</SelectItem>
                <SelectItem value="90d">Last 90 days</SelectItem>
                <SelectItem value="1y">Last year</SelectItem>
              </SelectContent>
            </Select>
            
            <Popover>
              <PopoverTrigger asChild>
                <Button variant="outline">
                  <CalendarIcon className="h-4 w-4 mr-2" />
                  Custom Range
                </Button>
              </PopoverTrigger>
              <PopoverContent className="w-auto p-0" align="start">
                <Calendar mode="single" />
              </PopoverContent>
            </Popover>
          </div>

          <Button>
            <RefreshCw className="h-4 w-4 mr-2" />
            Refresh Data
          </Button>
        </div>

        <TabsContent value="dashboard" className="space-y-6">
          {/* KPI Summary */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {kpiData.map((kpi, index) => (
              <Card key={index}>
                <CardContent className="pt-6">
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="text-sm text-muted-foreground">{kpi.name}</p>
                      <p className="text-2xl font-medium">{kpi.value}</p>
                      <p className={`text-sm flex items-center gap-1 ${
                        kpi.trend === 'up' ? 'text-green-600' : 'text-red-600'
                      }`}>
                        <TrendingUp className={`h-3 w-3 ${kpi.trend === 'down' ? 'rotate-180' : ''}`} />
                        {kpi.change}
                      </p>
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>

          {/* Charts */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle>Production Efficiency Trends</CardTitle>
              </CardHeader>
              <CardContent>
                <ResponsiveContainer width="100%" height={300}>
                  <LineChart data={productionData}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="date" />
                    <YAxis />
                    <Tooltip />
                    <Line type="monotone" dataKey="ctlLine1" stroke="#0d9488" name="CTL Line 1" />
                    <Line type="monotone" dataKey="ctlLine2" stroke="#14b8a6" name="CTL Line 2" />
                    <Line type="monotone" dataKey="slitter1" stroke="#2dd4bf" name="Slitter 1" />
                    <Line type="monotone" dataKey="slitter2" stroke="#5eead4" name="Slitter 2" />
                  </LineChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Quality Metrics</CardTitle>
              </CardHeader>
              <CardContent>
                <ResponsiveContainer width="100%" height={300}>
                  <BarChart data={qualityData}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="month" />
                    <YAxis />
                    <Tooltip />
                    <Bar dataKey="defectRate" fill="#0d9488" name="Defect Rate %" />
                    <Bar dataKey="customerComplaints" fill="#14b8a6" name="Customer Complaints" />
                  </BarChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Shipping Performance</CardTitle>
              </CardHeader>
              <CardContent>
                <ResponsiveContainer width="100%" height={300}>
                  <BarChart data={shippingData}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="week" />
                    <YAxis />
                    <Tooltip />
                    <Bar dataKey="onTime" fill="#10b981" name="On Time %" />
                    <Bar dataKey="late" fill="#ef4444" name="Late %" />
                  </BarChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Inventory Distribution</CardTitle>
              </CardHeader>
              <CardContent>
                <ResponsiveContainer width="100%" height={300}>
                  <PieChart>
                    <Pie
                      data={inventoryData}
                      cx="50%"
                      cy="50%"
                      outerRadius={80}
                      dataKey="value"
                      label={({ name, value }) => `${name}: ${value}%`}
                    >
                      {inventoryData.map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={entry.color} />
                      ))}
                    </Pie>
                    <Tooltip />
                  </PieChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="reports" className="space-y-4">
          <div className="flex flex-wrap gap-4 items-center justify-between">
            <div className="flex gap-2 items-center">
              <Select value={selectedCategory} onValueChange={setSelectedCategory}>
                <SelectTrigger className="w-48">
                  <SelectValue placeholder="Filter by category" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Categories</SelectItem>
                  <SelectItem value="production">Production</SelectItem>
                  <SelectItem value="quality">Quality</SelectItem>
                  <SelectItem value="shipping">Shipping</SelectItem>
                  <SelectItem value="inventory">Inventory</SelectItem>
                  <SelectItem value="financial">Financial</SelectItem>
                  <SelectItem value="operational">Operational</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <Button>
              <FileText className="h-4 w-4 mr-2" />
              Create New Report
            </Button>
          </div>

          <div className="space-y-4">
            {filteredReports.map(report => (
              <Card key={report.id} className="hover:shadow-md transition-shadow">
                <CardHeader className="pb-3">
                  <div className="flex items-start justify-between">
                    <div>
                      <CardTitle className="text-lg flex items-center gap-2">
                        {getCategoryIcon(report.category)}
                        {report.name}
                      </CardTitle>
                      <p className="text-sm text-muted-foreground mt-1">{report.description}</p>
                    </div>
                    <div className="flex items-center gap-2">
                      <Badge variant="outline" className={getCategoryColor(report.category)}>
                        {report.category}
                      </Badge>
                      <Badge variant="outline" className="capitalize">
                        {report.type}
                      </Badge>
                    </div>
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="space-y-4">
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-4 text-sm">
                      <div>
                        <span className="text-muted-foreground">Frequency:</span>
                        <p className="font-medium capitalize">{report.frequency}</p>
                      </div>
                      <div>
                        <span className="text-muted-foreground">Format:</span>
                        <p className="font-medium uppercase">{report.format}</p>
                      </div>
                      <div>
                        <span className="text-muted-foreground">Last Generated:</span>
                        <p className="font-medium">{new Date(report.lastGenerated).toLocaleDateString()}</p>
                      </div>
                    </div>

                    <div className="text-sm">
                      <span className="text-muted-foreground">Recipients:</span>
                      <div className="flex flex-wrap gap-1 mt-1">
                        {report.recipients.map(recipient => (
                          <Badge key={recipient} variant="secondary" className="text-xs">
                            {recipient}
                          </Badge>
                        ))}
                      </div>
                    </div>

                    <div className="flex justify-end gap-2 pt-3 border-t">
                      <Button variant="outline" size="sm">
                        <Eye className="h-4 w-4 mr-2" />
                        Preview
                      </Button>
                      <Button variant="outline" size="sm">
                        <Settings className="h-4 w-4 mr-2" />
                        Configure
                      </Button>
                      <Button variant="outline" size="sm">
                        <Download className="h-4 w-4 mr-2" />
                        Download
                      </Button>
                      <Button size="sm">
                        <RefreshCw className="h-4 w-4 mr-2" />
                        Generate Now
                      </Button>
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </TabsContent>

        <TabsContent value="production" className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            <Card className="lg:col-span-2">
              <CardHeader>
                <CardTitle>Machine Utilization</CardTitle>
              </CardHeader>
              <CardContent>
                <ResponsiveContainer width="100%" height={400}>
                  <BarChart data={productionData}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="date" />
                    <YAxis />
                    <Tooltip />
                    <Bar dataKey="ctlLine1" fill="#0d9488" name="CTL Line 1" />
                    <Bar dataKey="ctlLine2" fill="#14b8a6" name="CTL Line 2" />
                    <Bar dataKey="slitter1" fill="#2dd4bf" name="Slitter 1" />
                    <Bar dataKey="slitter2" fill="#5eead4" name="Slitter 2" />
                  </BarChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>

            <div className="space-y-6">
              <Card>
                <CardHeader>
                  <CardTitle className="text-base">Production Summary</CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="space-y-3">
                    <div className="flex justify-between">
                      <span className="text-sm text-muted-foreground">Total Output</span>
                      <span className="font-medium">847,230 ft</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-sm text-muted-foreground">Avg Efficiency</span>
                      <span className="font-medium">89.2%</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-sm text-muted-foreground">Total Downtime</span>
                      <span className="font-medium">12.5 hrs</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-sm text-muted-foreground">Scrap Rate</span>
                      <span className="font-medium">2.1%</span>
                    </div>
                  </div>
                </CardContent>
              </Card>

              <Card>
                <CardHeader>
                  <CardTitle className="text-base">Top Performers</CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="space-y-2">
                    <div className="flex items-center justify-between">
                      <span className="text-sm">Slitter 2</span>
                      <Badge className="bg-green-100 text-green-800">93.4%</Badge>
                    </div>
                    <div className="flex items-center justify-between">
                      <span className="text-sm">CTL Line 1</span>
                      <Badge className="bg-green-100 text-green-800">91.8%</Badge>
                    </div>
                    <div className="flex items-center justify-between">
                      <span className="text-sm">Slitter 1</span>
                      <Badge className="bg-blue-100 text-blue-800">89.6%</Badge>
                    </div>
                    <div className="flex items-center justify-between">
                      <span className="text-sm">CTL Line 2</span>
                      <Badge className="bg-orange-100 text-orange-800">86.2%</Badge>
                    </div>
                  </div>
                </CardContent>
              </Card>
            </div>
          </div>
        </TabsContent>

        <TabsContent value="quality" className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle>Quality Trends</CardTitle>
              </CardHeader>
              <CardContent>
                <ResponsiveContainer width="100%" height={300}>
                  <LineChart data={qualityData}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="month" />
                    <YAxis />
                    <Tooltip />
                    <Line type="monotone" dataKey="defectRate" stroke="#ef4444" name="Defect Rate %" />
                    <Line type="monotone" dataKey="rework" stroke="#f97316" name="Rework %" />
                  </LineChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Customer Satisfaction</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="text-center">
                    <div className="text-4xl font-bold text-teal-600 mb-2">4.7/5</div>
                    <p className="text-sm text-muted-foreground">Average Rating</p>
                  </div>
                  
                  <div className="space-y-2">
                    <div className="flex items-center justify-between text-sm">
                      <span>5 Stars</span>
                      <div className="w-24 bg-gray-200 rounded-full h-2">
                        <div className="bg-green-500 h-2 rounded-full" style={{width: '75%'}}></div>
                      </div>
                      <span>75%</span>
                    </div>
                    <div className="flex items-center justify-between text-sm">
                      <span>4 Stars</span>
                      <div className="w-24 bg-gray-200 rounded-full h-2">
                        <div className="bg-blue-500 h-2 rounded-full" style={{width: '20%'}}></div>
                      </div>
                      <span>20%</span>
                    </div>
                    <div className="flex items-center justify-between text-sm">
                      <span>3 Stars</span>
                      <div className="w-24 bg-gray-200 rounded-full h-2">
                        <div className="bg-yellow-500 h-2 rounded-full" style={{width: '3%'}}></div>
                      </div>
                      <span>3%</span>
                    </div>
                    <div className="flex items-center justify-between text-sm">
                      <span>2 Stars</span>
                      <div className="w-24 bg-gray-200 rounded-full h-2">
                        <div className="bg-orange-500 h-2 rounded-full" style={{width: '1%'}}></div>
                      </div>
                      <span>1%</span>
                    </div>
                    <div className="flex items-center justify-between text-sm">
                      <span>1 Star</span>
                      <div className="w-24 bg-gray-200 rounded-full h-2">
                        <div className="bg-red-500 h-2 rounded-full" style={{width: '1%'}}></div>
                      </div>
                      <span>1%</span>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            <Card>
              <CardContent className="pt-6">
                <div className="text-center">
                  <div className="text-2xl font-bold text-green-600 mb-1">98.8%</div>
                  <p className="text-sm text-muted-foreground">Quality Score</p>
                  <p className="text-xs text-green-600 mt-1">↗ +2.1%</p>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardContent className="pt-6">
                <div className="text-center">
                  <div className="text-2xl font-bold text-blue-600 mb-1">1.2%</div>
                  <p className="text-sm text-muted-foreground">Defect Rate</p>
                  <p className="text-xs text-green-600 mt-1">↘ -0.3%</p>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardContent className="pt-6">
                <div className="text-center">
                  <div className="text-2xl font-bold text-orange-600 mb-1">1.8%</div>
                  <p className="text-sm text-muted-foreground">Rework Rate</p>
                  <p className="text-xs text-green-600 mt-1">↘ -0.3%</p>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardContent className="pt-6">
                <div className="text-center">
                  <div className="text-2xl font-bold text-teal-600 mb-1">1</div>
                  <p className="text-sm text-muted-foreground">Customer Complaints</p>
                  <p className="text-xs text-green-600 mt-1">↘ -1</p>
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>
      </Tabs>
    </div>
  );
}
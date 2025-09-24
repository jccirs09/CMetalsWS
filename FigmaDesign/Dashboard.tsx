import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "./ui/card";
import { Badge } from "./ui/badge";
import { Button } from "./ui/button";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "./ui/tabs";
import { Progress } from "./ui/progress";
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  LineChart,
  Line
} from "recharts";
import {
  Activity,
  TrendingUp,
  Truck,
  Package,
  Clock,
  AlertTriangle,
  Play,
  Pause
} from "lucide-react";
import { StatusChip } from "./StatusChip";

const throughputData = [
  { time: "08:00", ctl: 1200, slitter: 800, pulling: 600 },
  { time: "10:00", ctl: 1500, slitter: 950, pulling: 750 },
  { time: "12:00", ctl: 1100, slitter: 700, pulling: 500 },
  { time: "14:00", ctl: 1800, slitter: 1200, pulling: 900 },
  { time: "16:00", ctl: 1600, slitter: 1000, pulling: 800 },
];

const truckData = [
  { day: "Mon", loads: 12, weight: 48000 },
  { day: "Tue", loads: 15, weight: 62000 },
  { day: "Wed", loads: 18, weight: 71000 },
  { day: "Thu", loads: 14, weight: 55000 },
  { day: "Fri", loads: 16, weight: 64000 },
];

const currentOrders = [
  {
    id: "WO-2024001",
    machine: "CTL Line 1", 
    customer: "Industrial Metals Co",
    product: "Hot Rolled Coil",
    gauge: "16 GA",
    weight: "2,400 lbs",
    progress: 65,
    operator: "Mike Johnson",
    started: "09:15 AM"
  },
  {
    id: "WO-2024002", 
    machine: "Slitter 2",
    customer: "Precision Parts LLC",
    product: "Cold Rolled Sheet",
    gauge: "20 GA", 
    weight: "1,800 lbs",
    progress: 30,
    operator: "Sarah Chen",
    started: "10:30 AM"
  }
];

const pullingOrders = [
  {
    id: "PO-2024045",
    customer: "Metro Construction",
    items: 12,
    weight: "3,200 lbs",
    destination: "Chicago North",
    operator: "Tom Wilson",
    progress: 75
  },
  {
    id: "PO-2024046",
    customer: "Steel Solutions Inc",
    items: 8,
    weight: "2,100 lbs", 
    destination: "Milwaukee",
    operator: "Lisa Rodriguez",
    progress: 20
  }
];

export function Dashboard() {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1>Operations Dashboard</h1>
          <p className="text-muted-foreground">
            Real-time view of warehouse operations and performance
          </p>
        </div>
        <Tabs defaultValue="daily" className="w-auto">
          <TabsList>
            <TabsTrigger value="daily">Daily</TabsTrigger>
            <TabsTrigger value="weekly">Weekly</TabsTrigger>
            <TabsTrigger value="monthly">Monthly</TabsTrigger>
          </TabsList>
        </Tabs>
      </div>

      {/* Key Metrics */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Daily Throughput</CardTitle>
            <Activity className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">24,800 lbs</div>
            <p className="text-xs text-muted-foreground">
              <span className="text-green-600">+12%</span> from yesterday
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Active Orders</CardTitle>
            <Package className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">18</div>
            <p className="text-xs text-muted-foreground">
              6 in progress, 12 queued
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Truck Loads</CardTitle>
            <Truck className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">6</div>
            <p className="text-xs text-muted-foreground">
              4 loaded, 2 in progress
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Machine Efficiency</CardTitle>
            <TrendingUp className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">94%</div>
            <p className="text-xs text-muted-foreground">
              <span className="text-green-600">+2%</span> vs target
            </p>
          </CardContent>
        </Card>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        {/* Machine Throughput Chart */}
        <Card>
          <CardHeader>
            <CardTitle>Machine Throughput (lbs/hour)</CardTitle>
            <CardDescription>Real-time production rates by machine</CardDescription>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <LineChart data={throughputData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="time" />
                <YAxis />
                <Tooltip />
                <Line type="monotone" dataKey="ctl" stroke="#0d9488" strokeWidth={2} name="CTL" />
                <Line type="monotone" dataKey="slitter" stroke="#3b82f6" strokeWidth={2} name="Slitter" />
                <Line type="monotone" dataKey="pulling" stroke="#10b981" strokeWidth={2} name="Pulling" />
              </LineChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>

        {/* Weekly Truck Loads */}
        <Card>
          <CardHeader>
            <CardTitle>Weekly Truck Loads</CardTitle>
            <CardDescription>Daily shipping volume and weight</CardDescription>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={truckData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="day" />
                <YAxis />
                <Tooltip />
                <Bar dataKey="loads" fill="#0d9488" name="Loads" />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>
      </div>

      {/* Now Playing Section */}
      <div className="grid gap-6 lg:grid-cols-2">
        {/* Current Machine Orders */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Play className="h-4 w-4 text-teal-600" />
              Now Playing - Machines
            </CardTitle>
            <CardDescription>Current orders in production</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            {currentOrders.map((order) => (
              <div key={order.id} className="space-y-3 p-4 border rounded-lg">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <Badge variant="outline">{order.id}</Badge>
                    <StatusChip status="in-progress" />
                  </div>
                  <Badge className="bg-teal-100 text-teal-700">{order.machine}</Badge>
                </div>
                
                <div className="grid grid-cols-2 gap-4 text-sm">
                  <div>
                    <p className="text-muted-foreground">Customer</p>
                    <p className="font-medium">{order.customer}</p>
                  </div>
                  <div>
                    <p className="text-muted-foreground">Product</p>
                    <p className="font-medium">{order.product}</p>
                  </div>
                  <div>
                    <p className="text-muted-foreground">Gauge</p>
                    <p className="font-medium">{order.gauge}</p>
                  </div>
                  <div>
                    <p className="text-muted-foreground">Weight</p>
                    <p className="font-medium">{order.weight}</p>
                  </div>
                </div>

                <div className="space-y-2">
                  <div className="flex justify-between text-sm">
                    <span>Progress</span>
                    <span>{order.progress}%</span>
                  </div>
                  <Progress value={order.progress} className="h-2" />
                </div>

                <div className="flex items-center justify-between text-sm text-muted-foreground">
                  <span>Operator: {order.operator}</span>
                  <span>Started: {order.started}</span>
                </div>
              </div>
            ))}
          </CardContent>
        </Card>

        {/* Current Pulling Sessions */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Package className="h-4 w-4 text-blue-600" />
              Now Playing - Pulling
            </CardTitle>
            <CardDescription>Current picking and packing sessions</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            {pullingOrders.map((order) => (
              <div key={order.id} className="space-y-3 p-4 border rounded-lg">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <Badge variant="outline">{order.id}</Badge>
                    <StatusChip status="in-progress" />
                  </div>
                  <Badge className="bg-blue-100 text-blue-700">{order.destination}</Badge>
                </div>
                
                <div className="grid grid-cols-2 gap-4 text-sm">
                  <div>
                    <p className="text-muted-foreground">Customer</p>
                    <p className="font-medium">{order.customer}</p>
                  </div>
                  <div>
                    <p className="text-muted-foreground">Items</p>
                    <p className="font-medium">{order.items} line items</p>
                  </div>
                  <div>
                    <p className="text-muted-foreground">Weight</p>
                    <p className="font-medium">{order.weight}</p>
                  </div>
                  <div>
                    <p className="text-muted-foreground">Operator</p>
                    <p className="font-medium">{order.operator}</p>
                  </div>
                </div>

                <div className="space-y-2">
                  <div className="flex justify-between text-sm">
                    <span>Progress</span>
                    <span>{order.progress}%</span>
                  </div>
                  <Progress value={order.progress} className="h-2" />
                </div>
              </div>
            ))}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
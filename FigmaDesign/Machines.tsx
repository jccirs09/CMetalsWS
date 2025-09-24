import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "./ui/card";
import { Button } from "./ui/button";
import { Badge } from "./ui/badge";
import { Input } from "./ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "./ui/select";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "./ui/tabs";
import { Progress } from "./ui/progress";
import { 
  Settings, 
  Activity, 
  AlertCircle,
  CheckCircle2,
  Clock,
  Gauge,
  Thermometer,
  Zap,
  Wrench,
  Play,
  Pause,
  Square,
  RotateCcw,
  TrendingUp,
  Calendar,
  User,
  Package,
  Truck,
  Scissors,
  PackageOpen
} from "lucide-react";

interface MachineAlert {
  id: string;
  level: "info" | "warning" | "error";
  message: string;
  timestamp: string;
  acknowledged: boolean;
}

interface MaintenanceRecord {
  id: string;
  type: "scheduled" | "repair" | "emergency";
  description: string;
  performedBy: string;
  date: string;
  duration: number; // minutes
  cost: number;
  partsUsed: string[];
}

interface Machine {
  id: string;
  name: string;
  type: "ctl" | "slitter" | "picking";
  model: string;
  serialNumber: string;
  status: "running" | "idle" | "maintenance" | "error" | "offline";
  location: string;
  currentOperator?: string;
  currentWorkOrder?: string;
  specifications: {
    maxWidth: number;
    minWidth: number;
    maxGauge: string;
    minGauge: string;
    maxSpeed: number; // feet per minute
    maxCoilWeight: number; // pounds
  };
  currentMetrics: {
    speed: number;
    utilization: number;
    temperature: number;
    pressure?: number;
    vibration?: number;
    powerConsumption: number;
  };
  todaysProduction: {
    totalFeet: number;
    totalCoils: number;
    totalWeight: number;
    efficiency: number;
    downtime: number; // minutes
  };
  alerts: MachineAlert[];
  lastMaintenance: string;
  nextMaintenance: string;
  totalRunHours: number;
  maintenanceHistory: MaintenanceRecord[];
  installDate: string;
}

const sampleMachines: Machine[] = [
  {
    id: "CTL-001",
    name: "CTL Line 1",
    type: "ctl",
    model: "Bradbury 2000",
    serialNumber: "BR-2000-001",
    status: "running",
    location: "Building A - Bay 1",
    currentOperator: "Mike Rodriguez",
    currentWorkOrder: "WO-2024-0158",
    specifications: {
      maxWidth: 72,
      minWidth: 12,
      maxGauge: "10ga",
      minGauge: "26ga", 
      maxSpeed: 200,
      maxCoilWeight: 20000
    },
    currentMetrics: {
      speed: 145,
      utilization: 78,
      temperature: 165,
      pressure: 85,
      vibration: 2.3,
      powerConsumption: 125
    },
    todaysProduction: {
      totalFeet: 8420,
      totalCoils: 15,
      totalWeight: 45600,
      efficiency: 82,
      downtime: 25
    },
    alerts: [
      {
        id: "AL-001",
        level: "warning",
        message: "Hydraulic pressure slightly elevated",
        timestamp: "2024-01-15T10:30:00Z",
        acknowledged: false
      }
    ],
    lastMaintenance: "2024-01-01",
    nextMaintenance: "2024-02-01",
    totalRunHours: 12450,
    installDate: "2020-03-15",
    maintenanceHistory: [
      {
        id: "MR-001",
        type: "scheduled",
        description: "Monthly preventive maintenance - lubrication and inspection",
        performedBy: "Maintenance Team",
        date: "2024-01-01",
        duration: 120,
        cost: 450,
        partsUsed: ["Hydraulic fluid", "Grease cartridges"]
      }
    ]
  },
  {
    id: "CTL-002",
    name: "CTL Line 2", 
    type: "ctl",
    model: "Bradbury 1800",
    serialNumber: "BR-1800-002",
    status: "maintenance",
    location: "Building A - Bay 2",
    specifications: {
      maxWidth: 60,
      minWidth: 10,
      maxGauge: "12ga", 
      minGauge: "24ga",
      maxSpeed: 180,
      maxCoilWeight: 18000
    },
    currentMetrics: {
      speed: 0,
      utilization: 0,
      temperature: 72,
      pressure: 0,
      vibration: 0,
      powerConsumption: 0
    },
    todaysProduction: {
      totalFeet: 0,
      totalCoils: 0,
      totalWeight: 0,
      efficiency: 0,
      downtime: 480
    },
    alerts: [
      {
        id: "AL-002",
        level: "error",
        message: "Scheduled maintenance in progress",
        timestamp: "2024-01-15T06:00:00Z",
        acknowledged: true
      }
    ],
    lastMaintenance: "2024-01-15",
    nextMaintenance: "2024-02-15",
    totalRunHours: 8920,
    installDate: "2019-08-22",
    maintenanceHistory: [
      {
        id: "MR-002",
        type: "repair",
        description: "Replace worn cutting blades and recalibrate",
        performedBy: "John Smith",
        date: "2024-01-15",
        duration: 480,
        cost: 1250,
        partsUsed: ["Cutting blades set", "Calibration kit"]
      }
    ]
  },
  {
    id: "SLT-001",
    name: "Slitter 1",
    type: "slitter",
    model: "Yoder M3",
    serialNumber: "YD-M3-001",
    status: "idle",
    location: "Building B - Bay 1", 
    currentOperator: "Sarah Chen",
    specifications: {
      maxWidth: 84,
      minWidth: 6,
      maxGauge: "8ga",
      minGauge: "28ga",
      maxSpeed: 300,
      maxCoilWeight: 25000
    },
    currentMetrics: {
      speed: 0,
      utilization: 25,
      temperature: 78,
      vibration: 0.5,
      powerConsumption: 15
    },
    todaysProduction: {
      totalFeet: 3240,
      totalCoils: 8,
      totalWeight: 18200,
      efficiency: 65,
      downtime: 95
    },
    alerts: [],
    lastMaintenance: "2023-12-20",
    nextMaintenance: "2024-01-20",
    totalRunHours: 6750,
    installDate: "2021-06-10",
    maintenanceHistory: [
      {
        id: "MR-003",
        type: "scheduled",
        description: "Blade inspection and edge conditioning",
        performedBy: "Maintenance Team",
        date: "2023-12-20",
        duration: 90,
        cost: 320,
        partsUsed: ["Blade conditioning compound"]
      }
    ]
  },
  {
    id: "SLT-002",
    name: "Slitter 2",
    type: "slitter", 
    model: "Yoder M2",
    serialNumber: "YD-M2-002",
    status: "running",
    location: "Building B - Bay 2",
    currentOperator: "Carlos Martinez",
    currentWorkOrder: "WO-2024-0157",
    specifications: {
      maxWidth: 72,
      minWidth: 8,
      maxGauge: "10ga",
      minGauge: "26ga",
      maxSpeed: 250,
      maxCoilWeight: 20000
    },
    currentMetrics: {
      speed: 185,
      utilization: 92,
      temperature: 95,
      vibration: 1.8,
      powerConsumption: 98
    },
    todaysProduction: {
      totalFeet: 12350,
      totalCoils: 22,
      totalWeight: 52400,
      efficiency: 89,
      downtime: 15
    },
    alerts: [],
    lastMaintenance: "2024-01-05",
    nextMaintenance: "2024-02-05",
    totalRunHours: 5420,
    installDate: "2022-02-18",
    maintenanceHistory: []
  },
  {
    id: "PICK-001",
    name: "Picking Station 1",
    type: "picking",
    model: "Custom Station A",
    serialNumber: "PS-A-001",
    status: "running",
    location: "Building C - Station 1",
    currentOperator: "Maria Santos",
    specifications: {
      maxWidth: 96,
      minWidth: 4,
      maxGauge: "6ga",
      minGauge: "30ga",
      maxSpeed: 50,
      maxCoilWeight: 30000
    },
    currentMetrics: {
      speed: 35,
      utilization: 88,
      temperature: 72,
      powerConsumption: 25
    },
    todaysProduction: {
      totalFeet: 2840,
      totalCoils: 12,
      totalWeight: 28400,
      efficiency: 91,
      downtime: 8
    },
    alerts: [],
    lastMaintenance: "2024-01-10",
    nextMaintenance: "2024-01-24",
    totalRunHours: 3250,
    installDate: "2022-11-05",
    maintenanceHistory: []
  }
];

const getMachineIcon = (type: string) => {
  switch (type) {
    case "ctl": return <Truck className="h-5 w-5" />;
    case "slitter": return <Scissors className="h-5 w-5" />;  
    case "picking": return <PackageOpen className="h-5 w-5" />;
    default: return <Settings className="h-5 w-5" />;
  }
};

const getStatusColor = (status: string) => {
  switch (status) {
    case "running": return "bg-green-100 text-green-800 border-green-200";
    case "idle": return "bg-yellow-100 text-yellow-800 border-yellow-200";
    case "maintenance": return "bg-blue-100 text-blue-800 border-blue-200";
    case "error": return "bg-red-100 text-red-800 border-red-200";
    case "offline": return "bg-gray-100 text-gray-800 border-gray-200";
    default: return "bg-gray-100 text-gray-800 border-gray-200";
  }
};

const getAlertColor = (level: string) => {
  switch (level) {
    case "info": return "bg-blue-100 text-blue-800 border-blue-200";
    case "warning": return "bg-yellow-100 text-yellow-800 border-yellow-200";
    case "error": return "bg-red-100 text-red-800 border-red-200";
    default: return "bg-gray-100 text-gray-800 border-gray-200";
  }
};

export function Machines() {
  const [activeTab, setActiveTab] = useState("overview");
  const [selectedMachine, setSelectedMachine] = useState<Machine | null>(null);

  const runningMachines = sampleMachines.filter(m => m.status === 'running');
  const totalProduction = sampleMachines.reduce((sum, m) => sum + m.todaysProduction.totalFeet, 0);
  const averageEfficiency = sampleMachines.reduce((sum, m) => sum + m.todaysProduction.efficiency, 0) / sampleMachines.length;
  const totalDowntime = sampleMachines.reduce((sum, m) => sum + m.todaysProduction.downtime, 0);

  return (
    <div className="space-y-6">
      <div>
        <h1>Machine Management</h1>
        <p className="text-muted-foreground">Monitor and configure CTL, Slitter, and Picking stations</p>
      </div>

      <Tabs value={activeTab} onValueChange={setActiveTab} className="space-y-6">
        <TabsList className="grid w-full grid-cols-4">
          <TabsTrigger value="overview" className="flex items-center gap-2">
            <Activity className="h-4 w-4" />
            Overview
          </TabsTrigger>
          <TabsTrigger value="machines" className="flex items-center gap-2">
            <Settings className="h-4 w-4" />
            Machines
          </TabsTrigger>
          <TabsTrigger value="maintenance" className="flex items-center gap-2">
            <Wrench className="h-4 w-4" />
            Maintenance
          </TabsTrigger>
          <TabsTrigger value="alerts" className="flex items-center gap-2">
            <AlertCircle className="h-4 w-4" />
            Alerts
          </TabsTrigger>
        </TabsList>

        <TabsContent value="overview" className="space-y-6">
          {/* KPI Cards */}
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <Card>
              <CardContent className="pt-6">
                <div className="flex items-center">
                  <div className="p-2 bg-green-100 rounded-full">
                    <CheckCircle2 className="h-4 w-4 text-green-600" />
                  </div>
                  <div className="ml-4">
                    <p className="text-sm text-muted-foreground">Machines Running</p>
                    <p className="text-2xl font-medium">{runningMachines.length}/{sampleMachines.length}</p>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardContent className="pt-6">
                <div className="flex items-center">
                  <div className="p-2 bg-blue-100 rounded-full">
                    <TrendingUp className="h-4 w-4 text-blue-600" />
                  </div>
                  <div className="ml-4">
                    <p className="text-sm text-muted-foreground">Today's Production</p>
                    <p className="text-2xl font-medium">{totalProduction.toLocaleString()} ft</p>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardContent className="pt-6">
                <div className="flex items-center">
                  <div className="p-2 bg-teal-100 rounded-full">
                    <Gauge className="h-4 w-4 text-teal-600" />
                  </div>
                  <div className="ml-4">
                    <p className="text-sm text-muted-foreground">Avg Efficiency</p>
                    <p className="text-2xl font-medium">{averageEfficiency.toFixed(1)}%</p>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardContent className="pt-6">
                <div className="flex items-center">
                  <div className="p-2 bg-orange-100 rounded-full">
                    <Clock className="h-4 w-4 text-orange-600" />
                  </div>
                  <div className="ml-4">
                    <p className="text-sm text-muted-foreground">Total Downtime</p>
                    <p className="text-2xl font-medium">{Math.floor(totalDowntime / 60)}h {totalDowntime % 60}m</p>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Machine Status Grid */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {sampleMachines.map(machine => (
              <Card key={machine.id} className="hover:shadow-md transition-shadow cursor-pointer">
                <CardHeader className="pb-3">
                  <div className="flex items-center justify-between">
                    <CardTitle className="flex items-center gap-2">
                      {getMachineIcon(machine.type)}
                      {machine.name}
                    </CardTitle>
                    <Badge variant="outline" className={getStatusColor(machine.status)}>
                      {machine.status}
                    </Badge>
                  </div>
                  <p className="text-sm text-muted-foreground">{machine.model}</p>
                </CardHeader>
                <CardContent>
                  <div className="space-y-4">
                    {/* Current Metrics */}
                    <div className="grid grid-cols-2 gap-4 text-sm">
                      <div>
                        <span className="text-muted-foreground">Speed:</span>
                        <p className="font-medium">{machine.currentMetrics.speed} ft/min</p>
                      </div>
                      <div>
                        <span className="text-muted-foreground">Utilization:</span>
                        <p className="font-medium">{machine.currentMetrics.utilization}%</p>
                      </div>
                      <div>
                        <span className="text-muted-foreground">Temperature:</span>
                        <p className="font-medium">{machine.currentMetrics.temperature}°F</p>
                      </div>
                      <div>
                        <span className="text-muted-foreground">Power:</span>
                        <p className="font-medium">{machine.currentMetrics.powerConsumption} kW</p>
                      </div>
                    </div>

                    {/* Production Progress */}
                    <div className="space-y-2">
                      <div className="flex justify-between text-sm">
                        <span>Today's Efficiency</span>
                        <span>{machine.todaysProduction.efficiency}%</span>
                      </div>
                      <Progress value={machine.todaysProduction.efficiency} className="h-2" />
                    </div>

                    {/* Current Work */}
                    {machine.currentOperator && (
                      <div className="text-sm">
                        <span className="text-muted-foreground">Operator:</span>
                        <p className="font-medium">{machine.currentOperator}</p>
                      </div>
                    )}

                    {machine.currentWorkOrder && (
                      <div className="text-sm">
                        <span className="text-muted-foreground">Work Order:</span>
                        <p className="font-medium text-blue-600">{machine.currentWorkOrder}</p>
                      </div>
                    )}

                    {/* Alerts */}
                    {machine.alerts.length > 0 && (
                      <div className="space-y-1">
                        {machine.alerts.map(alert => (
                          <div key={alert.id} className={`text-xs p-2 rounded border ${getAlertColor(alert.level)}`}>
                            {alert.message}
                          </div>
                        ))}
                      </div>
                    )}
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </TabsContent>

        <TabsContent value="machines" className="space-y-4">
          {/* Machine Details */}
          <div className="space-y-4">
            {sampleMachines.map(machine => (
              <Card key={machine.id}>
                <CardHeader>
                  <div className="flex items-center justify-between">
                    <CardTitle className="flex items-center gap-2">
                      {getMachineIcon(machine.type)}
                      {machine.name}
                      <Badge variant="outline" className="capitalize">
                        {machine.type}
                      </Badge>
                    </CardTitle>
                    <div className="flex items-center gap-2">
                      <Badge variant="outline" className={getStatusColor(machine.status)}>
                        {machine.status}
                      </Badge>
                      <div className="flex gap-1">
                        {machine.status === 'running' && (
                          <Button variant="outline" size="sm">
                            <Pause className="h-4 w-4" />
                          </Button>
                        )}
                        {machine.status === 'idle' && (
                          <Button variant="outline" size="sm">
                            <Play className="h-4 w-4" />
                          </Button>
                        )}
                        <Button variant="outline" size="sm">
                          <Square className="h-4 w-4" />
                        </Button>
                        <Button variant="outline" size="sm">
                          <RotateCcw className="h-4 w-4" />
                        </Button>
                      </div>
                    </div>
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                    {/* Specifications */}
                    <div className="space-y-3">
                      <h4 className="font-medium">Specifications</h4>
                      <div className="space-y-2 text-sm">
                        <div>
                          <span className="text-muted-foreground">Model:</span>
                          <p className="font-medium">{machine.model}</p>
                        </div>
                        <div>
                          <span className="text-muted-foreground">Serial Number:</span>
                          <p className="font-medium">{machine.serialNumber}</p>
                        </div>
                        <div>
                          <span className="text-muted-foreground">Width Range:</span>
                          <p className="font-medium">{machine.specifications.minWidth}" - {machine.specifications.maxWidth}"</p>
                        </div>
                        <div>
                          <span className="text-muted-foreground">Gauge Range:</span>
                          <p className="font-medium">{machine.specifications.minGauge} - {machine.specifications.maxGauge}</p>
                        </div>
                        <div>
                          <span className="text-muted-foreground">Max Speed:</span>
                          <p className="font-medium">{machine.specifications.maxSpeed} ft/min</p>
                        </div>
                      </div>
                    </div>

                    {/* Current Metrics */}
                    <div className="space-y-3">
                      <h4 className="font-medium">Current Metrics</h4>
                      <div className="space-y-3">
                        <div className="space-y-1">
                          <div className="flex justify-between text-sm">
                            <span>Speed</span>
                            <span>{machine.currentMetrics.speed}/{machine.specifications.maxSpeed} ft/min</span>
                          </div>
                          <Progress value={(machine.currentMetrics.speed / machine.specifications.maxSpeed) * 100} className="h-2" />
                        </div>
                        
                        <div className="space-y-1">
                          <div className="flex justify-between text-sm">
                            <span>Utilization</span>
                            <span>{machine.currentMetrics.utilization}%</span>
                          </div>
                          <Progress value={machine.currentMetrics.utilization} className="h-2" />
                        </div>

                        <div className="grid grid-cols-2 gap-4 text-sm">
                          <div>
                            <span className="text-muted-foreground">Temperature:</span>
                            <p className="font-medium flex items-center gap-1">
                              <Thermometer className="h-3 w-3" />
                              {machine.currentMetrics.temperature}°F
                            </p>
                          </div>
                          <div>
                            <span className="text-muted-foreground">Power:</span>
                            <p className="font-medium flex items-center gap-1">
                              <Zap className="h-3 w-3" />
                              {machine.currentMetrics.powerConsumption} kW
                            </p>
                          </div>
                        </div>
                      </div>
                    </div>

                    {/* Production Today */}
                    <div className="space-y-3">
                      <h4 className="font-medium">Today's Production</h4>
                      <div className="space-y-2 text-sm">
                        <div>
                          <span className="text-muted-foreground">Total Feet:</span>
                          <p className="font-medium">{machine.todaysProduction.totalFeet.toLocaleString()} ft</p>
                        </div>
                        <div>
                          <span className="text-muted-foreground">Total Coils:</span>
                          <p className="font-medium">{machine.todaysProduction.totalCoils}</p>
                        </div>
                        <div>
                          <span className="text-muted-foreground">Total Weight:</span>
                          <p className="font-medium">{machine.todaysProduction.totalWeight.toLocaleString()} lbs</p>
                        </div>
                        <div>
                          <span className="text-muted-foreground">Efficiency:</span>
                          <p className="font-medium">{machine.todaysProduction.efficiency}%</p>
                        </div>
                        <div>
                          <span className="text-muted-foreground">Downtime:</span>
                          <p className="font-medium">{machine.todaysProduction.downtime} min</p>
                        </div>
                      </div>
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </TabsContent>

        <TabsContent value="maintenance" className="space-y-4">
          <div className="space-y-4">
            {sampleMachines.map(machine => (
              <Card key={machine.id}>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <Wrench className="h-5 w-5" />
                    {machine.name} - Maintenance Schedule
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="space-y-4">
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                      <div>
                        <span className="text-muted-foreground text-sm">Last Maintenance:</span>
                        <p className="font-medium">{new Date(machine.lastMaintenance).toLocaleDateString()}</p>
                      </div>
                      <div>
                        <span className="text-muted-foreground text-sm">Next Maintenance:</span>
                        <p className="font-medium">{new Date(machine.nextMaintenance).toLocaleDateString()}</p>
                      </div>
                      <div>
                        <span className="text-muted-foreground text-sm">Total Run Hours:</span>
                        <p className="font-medium">{machine.totalRunHours.toLocaleString()}</p>
                      </div>
                    </div>

                    {machine.maintenanceHistory.length > 0 && (
                      <div className="space-y-2">
                        <h4 className="font-medium">Recent Maintenance</h4>
                        {machine.maintenanceHistory.slice(0, 3).map(record => (
                          <div key={record.id} className="p-3 bg-gray-50 rounded-lg">
                            <div className="flex items-start justify-between">
                              <div>
                                <p className="font-medium text-sm">{record.description}</p>
                                <p className="text-xs text-muted-foreground">
                                  {new Date(record.date).toLocaleDateString()} • {record.performedBy} • {record.duration} min
                                </p>
                                {record.partsUsed.length > 0 && (
                                  <p className="text-xs text-muted-foreground mt-1">
                                    Parts: {record.partsUsed.join(", ")}
                                  </p>
                                )}
                              </div>
                              <div className="text-right">
                                <Badge variant="outline" className={`capitalize ${
                                  record.type === 'emergency' ? 'border-red-200 text-red-800' :
                                  record.type === 'repair' ? 'border-orange-200 text-orange-800' :
                                  'border-blue-200 text-blue-800'
                                }`}>
                                  {record.type}
                                </Badge>
                                <p className="text-sm font-medium mt-1">${record.cost}</p>
                              </div>
                            </div>
                          </div>
                        ))}
                      </div>
                    )}
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </TabsContent>

        <TabsContent value="alerts" className="space-y-4">
          <div className="space-y-4">
            {sampleMachines
              .filter(machine => machine.alerts.length > 0)
              .map(machine => (
              <Card key={machine.id}>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <AlertCircle className="h-5 w-5" />
                    {machine.name} - Active Alerts
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="space-y-3">
                    {machine.alerts.map(alert => (
                      <div key={alert.id} className={`p-3 rounded-lg border ${getAlertColor(alert.level)}`}>
                        <div className="flex items-start justify-between">
                          <div>
                            <p className="font-medium">{alert.message}</p>
                            <p className="text-sm opacity-80">
                              {new Date(alert.timestamp).toLocaleString()}
                            </p>
                          </div>
                          <div className="flex gap-2">
                            <Badge variant="outline" className={getAlertColor(alert.level)}>
                              {alert.level}
                            </Badge>
                            {!alert.acknowledged && (
                              <Button variant="outline" size="sm">
                                Acknowledge
                              </Button>
                            )}
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                </CardContent>
              </Card>
            ))}

            {sampleMachines.every(machine => machine.alerts.length === 0) && (
              <Card>
                <CardContent className="pt-6">
                  <div className="text-center">
                    <CheckCircle2 className="h-12 w-12 mx-auto text-green-500 mb-4" />
                    <h3 className="text-lg font-medium text-green-800 mb-2">All Clear!</h3>
                    <p className="text-green-600">No active alerts at this time.</p>
                  </div>
                </CardContent>
              </Card>
            )}
          </div>
        </TabsContent>
      </Tabs>
    </div>
  );
}
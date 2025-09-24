import { useState, useMemo } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "./ui/card";
import { Button } from "./ui/button";
import { Badge } from "./ui/badge";
import { StatusChip } from "./StatusChip";
import { ChevronLeft, ChevronRight, Clock, Settings, Play, Pause, AlertTriangle } from "lucide-react";

interface WorkOrder {
  id: string;
  customerName: string;
  status: "planned" | "ready" | "in-progress" | "paused" | "completed" | "error";
  priority: "low" | "medium" | "high" | "urgent";
  startTime: string;
  endTime: string;
  estimatedHours: number;
  actualHours?: number;
  operator?: string;
}

interface MachineStatus {
  id: string;
  name: string;
  type: "CTL" | "SLITTER" | "SHEET_PULLING" | "COIL_PULLING";
  status: "running" | "idle" | "maintenance" | "error";
  currentOperator?: string;
  efficiency: number;
  currentJob?: string;
}

const machineStatuses: MachineStatus[] = [
  {
    id: "ctl-001",
    name: "CTL Line #1",
    type: "CTL",
    status: "running",
    currentOperator: "Mike Johnson",
    efficiency: 87,
    currentJob: "WO-2024-1001"
  },
  {
    id: "ctl-002", 
    name: "CTL Line #2",
    type: "CTL",
    status: "idle",
    efficiency: 0
  },
  {
    id: "slitter-001",
    name: "Slitter #1",
    type: "SLITTER", 
    status: "running",
    currentOperator: "Sarah Chen",
    efficiency: 92,
    currentJob: "WO-2024-1003"
  },
  {
    id: "sheet-pull-001",
    name: "Sheet Pulling #1",
    type: "SHEET_PULLING",
    status: "running",
    currentOperator: "Carlos Rodriguez",
    efficiency: 78,
    currentJob: "WO-2024-1005"
  }
];

const workOrders: WorkOrder[] = [
  {
    id: "WO-2024-1001",
    customerName: "Ace Manufacturing",
    status: "in-progress",
    priority: "high",
    startTime: "08:00",
    endTime: "12:00",
    estimatedHours: 4.0,
    actualHours: 2.5,
    operator: "Mike Johnson"
  },
  {
    id: "WO-2024-1002",
    customerName: "BuildCorp",
    status: "ready",
    priority: "medium",
    startTime: "12:30",
    endTime: "15:30",
    estimatedHours: 3.0,
    operator: "Mike Johnson"
  },
  {
    id: "WO-2024-1003",
    customerName: "Metro Steel",
    status: "in-progress", 
    priority: "urgent",
    startTime: "07:30",
    endTime: "14:30",
    estimatedHours: 7.0,
    actualHours: 4.2,
    operator: "Sarah Chen"
  }
];

export function MachineScheduler() {
  const [selectedDate, setSelectedDate] = useState(new Date());
  const [selectedMachine, setSelectedMachine] = useState<string | null>(null);

  const formatDate = (date: Date) => {
    return date.toLocaleDateString("en-US", {
      weekday: "long",
      year: "numeric", 
      month: "long",
      day: "numeric"
    });
  };

  const getStatusIcon = (status: MachineStatus["status"]) => {
    switch (status) {
      case "running":
        return <Play className="h-4 w-4 text-green-600" />;
      case "idle":
        return <Pause className="h-4 w-4 text-gray-400" />;
      case "maintenance":
        return <Settings className="h-4 w-4 text-yellow-600" />;
      case "error":
        return <AlertTriangle className="h-4 w-4 text-red-600" />;
    }
  };

  const navigateDate = (direction: "prev" | "next") => {
    const newDate = new Date(selectedDate);
    newDate.setDate(selectedDate.getDate() + (direction === "next" ? 1 : -1));
    setSelectedDate(newDate);
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1>Machine Daily Schedule</h1>
          <p className="text-muted-foreground">
            Production scheduling for CTL, Slitter, and Pulling operations
          </p>
        </div>
        
        <div className="flex items-center gap-4">
          <div className="flex items-center gap-2">
            <Button variant="outline" size="sm" onClick={() => navigateDate("prev")}>
              <ChevronLeft className="h-4 w-4" />
            </Button>
            <div className="px-4 py-2 bg-teal-50 rounded-lg min-w-[250px] text-center">
              <span className="font-medium">{formatDate(selectedDate)}</span>
            </div>
            <Button variant="outline" size="sm" onClick={() => navigateDate("next")}>
              <ChevronRight className="h-4 w-4" />
            </Button>
          </div>
          
          <Button variant="outline" size="sm" onClick={() => setSelectedDate(new Date())}>
            Today
          </Button>
        </div>
      </div>

      {/* Machine Status Overview */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {machineStatuses.map((machine) => (
          <Card key={machine.id} className="cursor-pointer hover:shadow-md transition-shadow"
                onClick={() => setSelectedMachine(selectedMachine === machine.id ? null : machine.id)}>
            <CardHeader className="pb-3">
              <div className="flex items-center justify-between">
                <CardTitle className="text-sm">{machine.name}</CardTitle>
                {getStatusIcon(machine.status)}
              </div>
            </CardHeader>
            <CardContent className="space-y-3">
              <div className="flex items-center gap-2">
                <div className={`w-2 h-2 rounded-full ${
                  machine.status === "running" ? "bg-green-600" :
                  machine.status === "idle" ? "bg-gray-400" :
                  machine.status === "maintenance" ? "bg-yellow-600" :
                  "bg-red-600"
                }`} />
                <span className="text-xs text-muted-foreground capitalize">{machine.status}</span>
              </div>
              
              {machine.currentOperator && (
                <div className="text-xs">
                  <span className="text-muted-foreground">Operator: </span>
                  <span>{machine.currentOperator}</span>
                </div>
              )}
              
              <div className="flex items-center justify-between text-xs">
                <span className="text-muted-foreground">Efficiency</span>
                <span className={
                  machine.efficiency >= 80 ? "text-green-600" : 
                  machine.efficiency >= 60 ? "text-yellow-600" : 
                  "text-red-600"
                }>
                  {machine.efficiency}%
                </span>
              </div>
              
              {machine.currentJob && (
                <Badge variant="outline" className="text-xs">
                  {machine.currentJob}
                </Badge>
              )}
            </CardContent>
          </Card>
        ))}
      </div>

      {/* Work Orders List */}
      <Card>
        <CardHeader>
          <div className="flex items-center gap-2">
            <Clock className="h-5 w-5 text-teal-600" />
            <CardTitle>Work Orders</CardTitle>
            {selectedMachine && (
              <Badge variant="outline">
                Viewing: {machineStatuses.find(m => m.id === selectedMachine)?.name}
              </Badge>
            )}
          </div>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {workOrders.map((wo) => (
              <div key={wo.id} className="border rounded-lg p-4 space-y-3">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-3">
                    <span className="font-medium">{wo.id}</span>
                    <StatusChip status={wo.status} />
                    <Badge variant={wo.priority === "urgent" ? "destructive" : wo.priority === "high" ? "default" : "secondary"}>
                      {wo.priority}
                    </Badge>
                  </div>
                  <div className="text-sm text-muted-foreground">
                    {wo.startTime} - {wo.endTime} ({wo.estimatedHours}h)
                  </div>
                </div>
                
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <span className="text-sm text-muted-foreground">Customer: </span>
                    <span className="font-medium">{wo.customerName}</span>
                  </div>
                  <div>
                    <span className="text-sm text-muted-foreground">Operator: </span>
                    <span>{wo.operator || "Unassigned"}</span>
                  </div>
                </div>

                {wo.actualHours && (
                  <div className="flex justify-between text-sm">
                    <span className="text-muted-foreground">Progress: </span>
                    <span>{wo.actualHours}h / {wo.estimatedHours}h ({Math.round((wo.actualHours / wo.estimatedHours) * 100)}%)</span>
                  </div>
                )}
              </div>
            ))}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
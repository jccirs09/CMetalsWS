import { useState, useEffect } from "react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "./ui/card";
import { Button } from "./ui/button";
import { Badge } from "./ui/badge";
import { Input } from "./ui/input";
import { Label } from "./ui/label";
import { Textarea } from "./ui/textarea";
import { Progress } from "./ui/progress";
import { Separator } from "./ui/separator";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "./ui/tabs";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "./ui/select";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "./ui/dialog";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "./ui/alert-dialog";
import {
  Play,
  Pause,
  Square,
  Clock,
  User,
  Package,
  Settings,
  AlertTriangle,
  CheckCircle,
  Timer,
  Weight,
  Gauge,
  Ruler,
  BarChart3,
  MessageSquare,
  Camera,
  FileText,
  RefreshCw,
  ArrowLeft
} from "lucide-react";
import { StatusChip, Status } from "./StatusChip";
import { toast } from "sonner@2.0.3";

interface WorkOrder {
  id: string;
  salesOrder: string;
  customer: string;
  machine: string;
  product: string;
  gauge: string;
  width: string;
  length: string;
  weight: number;
  status: Status;
  priority: "low" | "normal" | "high" | "urgent";
  plannedStart: string;
  plannedEnd: string;
  actualStart?: string;
  actualEnd?: string;
  estimatedLbsPerHour: number;
  actualLbs?: number;
  operator?: string;
  dueDate: string;
  lineItems: Array<{
    id: string;
    product: string;
    gauge: string;
    width: string;
    length: string;
    quantity: number;
    weight: number;
    processedQuantity: number;
    status: "pending" | "in-progress" | "completed" | "error";
  }>;
  events: Array<{
    type: "created" | "started" | "paused" | "resumed" | "completed" | "error";
    timestamp: string;
    operator: string;
    notes?: string;
  }>;
}

const mockWorkOrder: WorkOrder = {
  id: "WO-2024005",
  salesOrder: "SO-45625",
  customer: "Industrial Metals Co",
  machine: "CTL Line 1",
  product: "Hot Rolled Coil - 16 GA x 48\" x 120\"",
  gauge: "16 GA",
  width: "48\"",
  length: "120\"",
  weight: 4800,
  status: "ready",
  priority: "normal",
  plannedStart: "2024-12-13 14:00",
  plannedEnd: "2024-12-13 18:30",
  estimatedLbsPerHour: 1000,
  actualLbs: 0,
  dueDate: "2024-12-13",
  lineItems: [
    {
      id: "LI-001",
      product: "Hot Rolled Sheet - 16 GA x 48\" x 120\"",
      gauge: "16 GA",
      width: "48\"",
      length: "120\"",
      quantity: 10,
      weight: 2400,
      processedQuantity: 0,
      status: "pending"
    },
    {
      id: "LI-002",
      product: "Hot Rolled Sheet - 16 GA x 48\" x 96\"",
      gauge: "16 GA",
      width: "48\"",
      length: "96\"",
      quantity: 15,
      weight: 2400,
      processedQuantity: 0,
      status: "pending"
    }
  ],
  events: [
    { type: "created", timestamp: "2024-12-12 16:30", operator: "Sarah Planning" }
  ]
};

interface WorkOrderProcessProps {
  onNavigate?: (page: string) => void;
}

export function WorkOrderProcess({ onNavigate }: WorkOrderProcessProps = {}) {
  const [workOrder, setWorkOrder] = useState<WorkOrder>(mockWorkOrder);
  const [currentOperator, setCurrentOperator] = useState("Mike Johnson");
  const [isProcessing, setIsProcessing] = useState(false);
  const [pauseReason, setPauseReason] = useState("");
  const [qualityNotes, setQualityNotes] = useState("");
  const [actualLbsInput, setActualLbsInput] = useState("");
  const [elapsedTime, setElapsedTime] = useState(0);
  const [activeLineItem, setActiveLineItem] = useState<string | null>(null);

  // Timer effect for tracking elapsed time
  useEffect(() => {
    let interval: NodeJS.Timeout;
    if (isProcessing && workOrder.status === "in-progress") {
      interval = setInterval(() => {
        setElapsedTime(prev => prev + 1);
      }, 1000);
    }
    return () => clearInterval(interval);
  }, [isProcessing, workOrder.status]);

  const formatTime = (seconds: number) => {
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    const secs = seconds % 60;
    return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  };

  const calculateProgress = () => {
    const totalWeight = workOrder.lineItems.reduce((sum, item) => sum + item.weight, 0);
    const processedWeight = workOrder.lineItems.reduce((sum, item) => {
      return sum + (item.weight * (item.processedQuantity / item.quantity));
    }, 0);
    return totalWeight > 0 ? (processedWeight / totalWeight) * 100 : 0;
  };

  const startWorkOrder = () => {
    const now = new Date().toISOString();
    setWorkOrder(prev => ({
      ...prev,
      status: "in-progress",
      actualStart: now,
      operator: currentOperator,
      events: [...prev.events, {
        type: "started",
        timestamp: now,
        operator: currentOperator
      }]
    }));
    setIsProcessing(true);
    toast.success("Work order started");
  };

  const pauseWorkOrder = (reason: string) => {
    const now = new Date().toISOString();
    setWorkOrder(prev => ({
      ...prev,
      status: "paused",
      events: [...prev.events, {
        type: "paused",
        timestamp: now,
        operator: currentOperator,
        notes: reason
      }]
    }));
    setIsProcessing(false);
    setPauseReason("");
    toast.success("Work order paused");
  };

  const resumeWorkOrder = () => {
    const now = new Date().toISOString();
    setWorkOrder(prev => ({
      ...prev,
      status: "in-progress",
      events: [...prev.events, {
        type: "resumed",
        timestamp: now,
        operator: currentOperator
      }]
    }));
    setIsProcessing(true);
    toast.success("Work order resumed");
  };

  const completeWorkOrder = () => {
    const now = new Date().toISOString();
    setWorkOrder(prev => ({
      ...prev,
      status: "completed",
      actualEnd: now,
      actualLbs: parseInt(actualLbsInput) || prev.actualLbs || prev.weight,
      events: [...prev.events, {
        type: "completed",
        timestamp: now,
        operator: currentOperator,
        notes: qualityNotes
      }]
    }));
    setIsProcessing(false);
    toast.success("Work order completed");
  };

  const updateLineItemProgress = (lineItemId: string, processedQty: number) => {
    setWorkOrder(prev => ({
      ...prev,
      lineItems: prev.lineItems.map(item => 
        item.id === lineItemId 
          ? { 
              ...item, 
              processedQuantity: Math.min(processedQty, item.quantity),
              status: processedQty >= item.quantity ? "completed" : 
                     processedQty > 0 ? "in-progress" : "pending"
            }
          : item
      )
    }));
  };

  const currentProgress = calculateProgress();
  const estimatedCompletion = workOrder.actualStart && workOrder.estimatedLbsPerHour > 0 
    ? new Date(new Date(workOrder.actualStart).getTime() + (workOrder.weight / workOrder.estimatedLbsPerHour * 60 * 60 * 1000))
    : null;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <div className="flex items-center gap-4 mb-2">
            <Button 
              variant="ghost" 
              size="sm"
              onClick={() => onNavigate?.("work-orders")}
            >
              <ArrowLeft className="h-4 w-4 mr-2" />
              Back
            </Button>
            <h1>{workOrder.id}</h1>
            <StatusChip status={workOrder.status} />
            <Badge className={
              workOrder.priority === "urgent" ? "bg-red-100 text-red-700" :
              workOrder.priority === "high" ? "bg-orange-100 text-orange-700" :
              workOrder.priority === "normal" ? "bg-blue-100 text-blue-700" :
              "bg-gray-100 text-gray-700"
            }>
              {workOrder.priority}
            </Badge>
          </div>
          <p className="text-muted-foreground">
            {workOrder.customer} • {workOrder.machine} • {workOrder.weight.toLocaleString()} lbs
          </p>
        </div>

        <div className="flex items-center gap-3">
          {workOrder.status === "ready" && (
            <Button onClick={startWorkOrder} className="bg-teal-600 hover:bg-teal-700">
              <Play className="h-4 w-4 mr-2" />
              Start Work Order
            </Button>
          )}
          
          {workOrder.status === "in-progress" && (
            <>
              <Dialog>
                <DialogTrigger asChild>
                  <Button variant="outline">
                    <Pause className="h-4 w-4 mr-2" />
                    Pause
                  </Button>
                </DialogTrigger>
                <DialogContent>
                  <DialogHeader>
                    <DialogTitle>Pause Work Order</DialogTitle>
                    <DialogDescription>Please provide a reason for pausing this work order</DialogDescription>
                  </DialogHeader>
                  <div className="space-y-4">
                    <div className="space-y-2">
                      <Label>Reason for Pause</Label>
                      <Select value={pauseReason} onValueChange={setPauseReason}>
                        <SelectTrigger>
                          <SelectValue placeholder="Select reason" />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="break">Break</SelectItem>
                          <SelectItem value="maintenance">Machine Maintenance</SelectItem>
                          <SelectItem value="material">Material Issue</SelectItem>
                          <SelectItem value="quality">Quality Check</SelectItem>
                          <SelectItem value="other">Other</SelectItem>
                        </SelectContent>
                      </Select>
                    </div>
                    <Button 
                      onClick={() => pauseWorkOrder(pauseReason)} 
                      disabled={!pauseReason}
                      className="w-full"
                    >
                      Pause Work Order
                    </Button>
                  </div>
                </DialogContent>
              </Dialog>

              <AlertDialog>
                <AlertDialogTrigger asChild>
                  <Button className="bg-green-600 hover:bg-green-700">
                    <CheckCircle className="h-4 w-4 mr-2" />
                    Complete
                  </Button>
                </AlertDialogTrigger>
                <AlertDialogContent>
                  <AlertDialogHeader>
                    <AlertDialogTitle>Complete Work Order</AlertDialogTitle>
                    <AlertDialogDescription>
                      Are you sure you want to mark this work order as complete?
                    </AlertDialogDescription>
                  </AlertDialogHeader>
                  <div className="space-y-4 my-4">
                    <div className="space-y-2">
                      <Label>Actual Pounds Processed</Label>
                      <Input
                        type="number"
                        placeholder={workOrder.weight.toString()}
                        value={actualLbsInput}
                        onChange={(e) => setActualLbsInput(e.target.value)}
                      />
                    </div>
                    <div className="space-y-2">
                      <Label>Quality Notes (Optional)</Label>
                      <Textarea
                        placeholder="Any quality observations or notes..."
                        value={qualityNotes}
                        onChange={(e) => setQualityNotes(e.target.value)}
                        rows={3}
                      />
                    </div>
                  </div>
                  <AlertDialogFooter>
                    <AlertDialogCancel>Cancel</AlertDialogCancel>
                    <AlertDialogAction onClick={completeWorkOrder}>
                      Complete Work Order
                    </AlertDialogAction>
                  </AlertDialogFooter>
                </AlertDialogContent>
              </AlertDialog>
            </>
          )}
          
          {workOrder.status === "paused" && (
            <Button onClick={resumeWorkOrder} className="bg-amber-600 hover:bg-amber-700">
              <Play className="h-4 w-4 mr-2" />
              Resume
            </Button>
          )}
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Main Processing Area */}
        <div className="lg:col-span-2 space-y-6">
          {/* Progress Overview */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <BarChart3 className="h-5 w-5" />
                Progress Overview
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <div className="flex justify-between text-sm">
                  <span>Overall Progress</span>
                  <span>{Math.round(currentProgress)}%</span>
                </div>
                <Progress value={currentProgress} className="h-2" />
              </div>
              
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
                <div className="text-center">
                  <div className="text-2xl font-bold text-teal-600">{formatTime(elapsedTime)}</div>
                  <div className="text-muted-foreground">Elapsed Time</div>
                </div>
                <div className="text-center">
                  <div className="text-2xl font-bold">{workOrder.actualLbs || 0}</div>
                  <div className="text-muted-foreground">Processed (lbs)</div>
                </div>
                <div className="text-center">
                  <div className="text-2xl font-bold">
                    {elapsedTime > 0 ? Math.round((workOrder.actualLbs || 0) / (elapsedTime / 3600)) : 0}
                  </div>
                  <div className="text-muted-foreground">Rate (lbs/hr)</div>
                </div>
                <div className="text-center">
                  <div className="text-2xl font-bold">
                    {estimatedCompletion ? estimatedCompletion.toLocaleTimeString().slice(0, 5) : "--:--"}
                  </div>
                  <div className="text-muted-foreground">Est. Complete</div>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Line Items Processing */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Package className="h-5 w-5" />
                Line Items ({workOrder.lineItems.length})
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {workOrder.lineItems.map((item) => (
                  <div key={item.id} className="border rounded-lg p-4 space-y-3">
                    <div className="flex items-center justify-between">
                      <div className="space-y-1">
                        <p className="font-medium">{item.product}</p>
                        <div className="flex items-center gap-4 text-sm text-muted-foreground">
                          <span className="flex items-center gap-1">
                            <Gauge className="h-3 w-3" />
                            {item.gauge}
                          </span>
                          <span className="flex items-center gap-1">
                            <Ruler className="h-3 w-3" />
                            {item.width} x {item.length}
                          </span>
                          <span className="flex items-center gap-1">
                            <Weight className="h-3 w-3" />
                            {item.weight.toLocaleString()} lbs
                          </span>
                        </div>
                      </div>
                      <StatusChip status={item.status} />
                    </div>

                    <div className="space-y-2">
                      <div className="flex justify-between text-sm">
                        <span>Progress: {item.processedQuantity} / {item.quantity}</span>
                        <span>{Math.round((item.processedQuantity / item.quantity) * 100)}%</span>
                      </div>
                      <Progress value={(item.processedQuantity / item.quantity) * 100} className="h-2" />
                    </div>

                    {workOrder.status === "in-progress" && (
                      <div className="flex items-center gap-2">
                        <Label className="text-sm">Processed Quantity:</Label>
                        <Input
                          type="number"
                          min="0"
                          max={item.quantity}
                          value={item.processedQuantity}
                          onChange={(e) => updateLineItemProgress(item.id, parseInt(e.target.value) || 0)}
                          className="w-20"
                        />
                        <span className="text-sm text-muted-foreground">/ {item.quantity}</span>
                      </div>
                    )}
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>

          {/* Quality Control */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <CheckCircle className="h-5 w-5" />
                Quality Control
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <Button variant="outline" className="h-auto p-4 flex flex-col items-center gap-2">
                  <Camera className="h-6 w-6" />
                  <span>Add Photo</span>
                </Button>
                <Button variant="outline" className="h-auto p-4 flex flex-col items-center gap-2">
                  <MessageSquare className="h-6 w-6" />
                  <span>Add Note</span>
                </Button>
              </div>
              
              {workOrder.status === "in-progress" && (
                <div className="space-y-2">
                  <Label>Quality Notes</Label>
                  <Textarea
                    placeholder="Enter quality observations, measurements, or issues..."
                    value={qualityNotes}
                    onChange={(e) => setQualityNotes(e.target.value)}
                    rows={3}
                  />
                </div>
              )}
            </CardContent>
          </Card>
        </div>

        {/* Sidebar - Work Order Details */}
        <div className="space-y-6">
          {/* Work Order Info */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <FileText className="h-5 w-5" />
                Work Order Details
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2 text-sm">
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Sales Order:</span>
                  <span>{workOrder.salesOrder}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Customer:</span>
                  <span>{workOrder.customer}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Machine:</span>
                  <span>{workOrder.machine}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Operator:</span>
                  <span>{workOrder.operator || "Not assigned"}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Due Date:</span>
                  <span>{workOrder.dueDate}</span>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Machine Status */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Settings className="h-5 w-5" />
                Machine Status
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex items-center justify-between">
                <span className="text-sm">Status</span>
                <Badge className="bg-green-100 text-green-700">Running</Badge>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm">Temperature</span>
                <span className="text-sm">68°F</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm">Speed</span>
                <span className="text-sm">95%</span>
              </div>
              <Separator />
              <Button variant="outline" size="sm" className="w-full">
                <RefreshCw className="h-4 w-4 mr-2" />
                Refresh Status
              </Button>
            </CardContent>
          </Card>

          {/* Timeline */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Clock className="h-5 w-5" />
                Timeline
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                {workOrder.events.map((event, index) => (
                  <div key={index} className="flex items-start gap-3">
                    <div className="flex h-6 w-6 items-center justify-center rounded-full bg-teal-100 mt-0.5">
                      {event.type === "created" && <Clock className="h-3 w-3 text-teal-600" />}
                      {event.type === "started" && <Play className="h-3 w-3 text-teal-600" />}
                      {event.type === "paused" && <Pause className="h-3 w-3 text-amber-600" />}
                      {event.type === "resumed" && <Play className="h-3 w-3 text-teal-600" />}
                      {event.type === "completed" && <CheckCircle className="h-3 w-3 text-green-600" />}
                      {event.type === "error" && <AlertTriangle className="h-3 w-3 text-red-600" />}
                    </div>
                    <div className="flex-1 text-sm">
                      <p className="font-medium capitalize">{event.type.replace("-", " ")}</p>
                      <p className="text-muted-foreground">
                        {new Date(event.timestamp).toLocaleString()}
                      </p>
                      <p className="text-muted-foreground">{event.operator}</p>
                      {event.notes && (
                        <p className="mt-1 text-muted-foreground italic">{event.notes}</p>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
"""
debate_engine/schemas.py
Strict JSON Handoff Dataclasses & Schemas for the 3 Opposing Power Chambers,
Parallel Debate Graph (DAG), and Evolution Synthesizer.
"""

from __future__ import annotations
import json
from dataclasses import dataclass, field, asdict
from typing import List, Optional, Dict, Any


@dataclass
class MetricImpact:
    latency_ms_delta: float = 0.0
    conversion_percent_delta: float = 0.0
    monthly_cost_usd_delta: float = 0.0
    resource_reserve_headroom_percent: float = 50.0

    def to_dict(self) -> Dict[str, Any]:
        return asdict(self)


@dataclass
class IssueItem:
    issue_id: str
    title: str
    severity: int  # 1 (Trivial) to 5 (Critical / P0)
    target_file: str
    description: str
    proposed_diff: str
    line_range: Optional[str] = None
    metric_impact: Optional[MetricImpact] = None
    concession: Optional[str] = None

    def to_dict(self) -> Dict[str, Any]:
        d = asdict(self)
        if self.metric_impact:
            d["metric_impact"] = self.metric_impact.to_dict()
        return d


@dataclass
class LensEvaluation:
    lens_id: str
    chamber: str
    status: str  # "PASS", "FLAGGED", "HARD_VETO"
    hard_veto_triggered: bool
    summary: str
    issues: List[IssueItem] = field(default_factory=list)
    hard_veto_rationale: Optional[str] = None

    def to_dict(self) -> Dict[str, Any]:
        return {
            "lens_id": self.lens_id,
            "chamber": self.chamber,
            "status": self.status,
            "hard_veto_triggered": self.hard_veto_triggered,
            "hard_veto_rationale": self.hard_veto_rationale,
            "issues": [issue.to_dict() for issue in self.issues],
            "summary": self.summary,
        }

    @classmethod
    def from_dict(cls, data: Dict[str, Any]) -> LensEvaluation:
        issues = []
        for item in data.get("issues", []):
            m_data = item.get("metric_impact")
            metric = MetricImpact(**m_data) if m_data else None
            issue = IssueItem(
                issue_id=item["issue_id"],
                title=item["title"],
                severity=int(item["severity"]),
                target_file=item["target_file"],
                description=item["description"],
                proposed_diff=item["proposed_diff"],
                line_range=item.get("line_range"),
                metric_impact=metric,
                concession=item.get("concession"),
            )
            issues.append(issue)
        return cls(
            lens_id=data["lens_id"],
            chamber=data["chamber"],
            status=data["status"],
            hard_veto_triggered=bool(data.get("hard_veto_triggered", False)),
            hard_veto_rationale=data.get("hard_veto_rationale"),
            summary=data.get("summary", ""),
            issues=issues,
        )


@dataclass
class ChamberParticipant:
    chamber_name: str
    advocate_lens: str
    position: str
    metric_claim: Optional[str] = None


@dataclass
class TradeOffResolution:
    issue_id: str
    severity: int
    concession_granted_by: str
    concession_terms: str
    approved_diff: str
    weighted_priority_score: Optional[float] = None


@dataclass
class TelemetryGrounding:
    reserve_headroom_observed_percent: float = 50.0
    estimated_monthly_spend_usd: float = 0.0
    gemini_token_budget_headroom_percent: float = 80.0
    is_within_budget_manifest: bool = True
    is_within_50_percent_reserve: bool = True


@dataclass
class DebateRoundPayload:
    round_id: str
    round_name: str
    chamber_a: ChamberParticipant
    chamber_b: ChamberParticipant
    hard_veto_asserted: bool
    trade_off_resolution: TradeOffResolution
    status: str  # "CONSENSUS_REACHED", "DEADLOCKED_VETO", "COMPROMISE_ACCEPTED"
    veto_details: Optional[str] = None
    telemetry_grounding: Optional[TelemetryGrounding] = None

    def to_dict(self) -> Dict[str, Any]:
        return {
            "round_id": self.round_id,
            "round_name": self.round_name,
            "chamber_a": asdict(self.chamber_a),
            "chamber_b": asdict(self.chamber_b),
            "hard_veto_asserted": self.hard_veto_asserted,
            "veto_details": self.veto_details,
            "trade_off_resolution": asdict(self.trade_off_resolution),
            "telemetry_grounding": asdict(self.telemetry_grounding) if self.telemetry_grounding else None,
            "status": self.status,
        }


@dataclass
class AutoRefactorDirective:
    directive_id: str
    target_file: str
    action: str  # "MODIFY", "CREATE", "DELETE", "MIGRATE"
    summary: str
    unified_diff: str
    compilation_impact: str  # "REQUIRES_DOTNET_BUILD", "RUNTIME_INTERPRETED_NO_BUILD"
    verification_command: str


@dataclass
class AntiObsolescenceGate:
    passed: bool
    target_framework: str = ".NET 8.0 (C# 12)"
    deprecated_apis_detected: List[str] = field(default_factory=list)
    binding_upgrades: List[Dict[str, str]] = field(default_factory=list)


@dataclass
class VetoRegistry:
    pen_tester_veto_active: bool = False
    legal_counsel_veto_active: bool = False
    blocking_reasons: List[str] = field(default_factory=list)


@dataclass
class ConsensusDecree:
    decree_id: str
    timestamp_utc: str
    verdict: str  # "CONSENSUS_DECREE_APPROVED", "CONSENSUS_DECREE_CONDITIONAL", "VETO_BLOCKED"
    anti_obsolescence_gate: AntiObsolescenceGate
    veto_registry: VetoRegistry
    weighted_trade_offs_summary: List[Dict[str, Any]]
    auto_refactor_directives: List[AutoRefactorDirective]

    def to_dict(self) -> Dict[str, Any]:
        return {
            "decree_id": self.decree_id,
            "timestamp_utc": self.timestamp_utc,
            "verdict": self.verdict,
            "anti_obsolescence_gate": asdict(self.anti_obsolescence_gate),
            "veto_registry": asdict(self.veto_registry),
            "weighted_trade_offs_summary": self.weighted_trade_offs_summary,
            "auto_refactor_directives": [asdict(d) for d in self.auto_refactor_directives],
        }

    def to_json(self, indent: int = 2) -> str:
        return json.dumps(self.to_dict(), indent=indent)

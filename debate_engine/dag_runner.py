"""
debate_engine/dag_runner.py
Master Asynchronous DAG Orchestrator implementing:
- Phase 1: Fan-out concurrent evaluation across the 10 Specialized Lenses.
- Phase 2: Isolated parallel 1v1 Debate Threads (Round 1, Round 2, Round 3).
- Phase 3: Fan-in Evolution Synthesizer (Anti-Obsolescence Gate & Binding Upgrades).
- Hard Veto Authority (Pen Tester & Legal Counsel) vs Weighted Scoring (Latency vs SEO).
- Grounded Telemetry (50% Reserve Invariant & Zero-Cost Cloud Constraints).
- Staged Review Mode Consensus Decrees + Auto-Refactor Directives.
"""

from __future__ import annotations
import asyncio
import json
import time
from datetime import datetime, timezone
from pathlib import Path
from typing import List, Dict, Any, Optional

from .schemas import (
    LensEvaluation,
    IssueItem,
    MetricImpact,
    DebateRoundPayload,
    ChamberParticipant,
    TradeOffResolution,
    TelemetryGrounding,
    ConsensusDecree,
    AntiObsolescenceGate,
    VetoRegistry,
    AutoRefactorDirective,
)
from .telemetry import TelemetryCollector, TelemetryReport
from .gemini_client import GeminiClient


class DebateGraphDAG:
    def __init__(self, workspace_root: Optional[Path] = None, use_mock: bool = False):
        self.root = workspace_root or Path(__file__).resolve().parent.parent
        self.use_mock = use_mock
        self.telemetry_collector = TelemetryCollector(self.root)
        self.gemini_client = GeminiClient(self.root)
        self.config = self._load_config()

    def _load_config(self) -> Dict[str, Any]:
        cfg_file = self.root / ".agents" / "config.json"
        if cfg_file.exists():
            with open(cfg_file, "r", encoding="utf-8") as f:
                return json.load(f)
        return {}

    def _load_lens_prompt(self, relative_path: str) -> str:
        prompt_file = self.root / relative_path
        if prompt_file.exists():
            with open(prompt_file, "r", encoding="utf-8") as f:
                return f.read()
        return f"# Lens System Prompt ({relative_path})"

    # --------------------------------------------------------------------------
    # PHASE 1: Concurrent Fan-out Lens Evaluations
    # --------------------------------------------------------------------------
    async def evaluate_single_lens(
        self,
        lens_info: Dict[str, Any],
        chamber_name: str,
        target_context: str
    ) -> LensEvaluation:
        """Run a single lens evaluation asynchronously."""
        lens_id = lens_info["id"]
        prompt_path = lens_info.get("prompt_path", "")
        system_prompt = self._load_lens_prompt(prompt_path)
        model = lens_info.get("model", "gemini-1.5-flash")

        user_prompt = (
            f"Target Context / Diff to evaluate for ExamiFy-Core:\n\n{target_context}\n\n"
            f"You are {lens_info['name']} ({lens_id}). Analyze against your invariants and output strict JSON."
        )

        resp_dict = await self.gemini_client.generate_json_async(
            prompt=user_prompt,
            system_instruction=system_prompt,
            model=model,
            use_mock=self.use_mock
        )

        # Ensure correct lens_id & chamber
        resp_dict["lens_id"] = lens_id
        resp_dict["chamber"] = chamber_name
        return LensEvaluation.from_dict(resp_dict)

    async def run_phase_1_fan_out(self, target_context: str) -> List[LensEvaluation]:
        """Execute all 10 lens evaluations concurrently in a fan-out phase."""
        tasks = []
        chambers = self.config.get("chambers", {})
        for chamber_key, chamber_data in chambers.items():
            c_name = chamber_data.get("name", chamber_key)
            for lens in chamber_data.get("lenses", []):
                tasks.append(self.evaluate_single_lens(lens, c_name, target_context))

        evaluations = await asyncio.gather(*tasks)
        return list(evaluations)

    # --------------------------------------------------------------------------
    # PHASE 2: Parallel Isolated 1v1 Debate Threads
    # --------------------------------------------------------------------------
    async def run_round_1_speed_vs_conversion(
        self,
        lens_evals: List[LensEvaluation]
    ) -> DebateRoundPayload:
        """Round 1: Speed vs Conversion (SEO Auditor & Monetizer vs Latency Hunter)."""
        seo_eval = next((e for e in lens_evals if e.lens_id == "the_seo_auditor"), None)
        latency_eval = next((e for e in lens_evals if e.lens_id == "the_latency_hunter"), None)

        # Extract metrics
        seo_issue = seo_eval.issues[0] if (seo_eval and seo_eval.issues) else None
        latency_issue = latency_eval.issues[0] if (latency_eval and latency_eval.issues) else None

        conv_lift = 20.0
        lat_penalty = 15.0

        if seo_issue and seo_issue.metric_impact:
            conv_lift = seo_issue.metric_impact.conversion_percent_delta or 20.0
            lat_penalty = seo_issue.metric_impact.latency_ms_delta or 15.0

        # Weighted Priority Formula: score = conv_lift / (lat_penalty / 15.0)
        norm_latency = max(lat_penalty / 15.0, 0.1)
        weighted_score = round(conv_lift / norm_latency, 2)

        # Threshold evaluation (e.g., tolerating a 15ms latency hit for a 20% conversion lift)
        if weighted_score >= 1.0:
            concession_by = "The Latency Hunter"
            concession_terms = (
                f"Accepted {lat_penalty}ms latency penalty in exchange for {conv_lift}% conversion lift "
                f"(Weighted Score: {weighted_score} >= 1.0 threshold)."
            )
            approved_diff = (seo_issue.proposed_diff if seo_issue else "// Approved async schema markup injection")
            status = "COMPROMISE_ACCEPTED"
        else:
            concession_by = "The SEO Auditor"
            concession_terms = "Refactored to client-side async script hydration to stay within 0ms server budget."
            approved_diff = "// Deferred async hydration"
            status = "CONSENSUS_REACHED"

        return DebateRoundPayload(
            round_id="round_1_speed_vs_conversion",
            round_name="Round 1: Speed vs. Conversion Debate",
            chamber_a=ChamberParticipant(
                chamber_name="Chamber 1: Commercial & Growth",
                advocate_lens="the_seo_auditor",
                position="Inject Schema.org and OpenGraph metadata to maximize organic candidate discovery.",
                metric_claim=f"+{conv_lift}% estimated organic conversion lift."
            ),
            chamber_b=ChamberParticipant(
                chamber_name="Chamber 2: Technical & Defense",
                advocate_lens="the_latency_hunter",
                position="Zero tolerance for server-side render latency additions > 15ms.",
                metric_claim=f"Requires TTFB latency delta <= {lat_penalty}ms."
            ),
            hard_veto_asserted=False,
            trade_off_resolution=TradeOffResolution(
                issue_id=seo_issue.issue_id if seo_issue else "TRADE-R1-001",
                severity=seo_issue.severity if seo_issue else 2,
                concession_granted_by=concession_by,
                concession_terms=concession_terms,
                approved_diff=approved_diff,
                weighted_priority_score=weighted_score
            ),
            status=status
        )

    async def run_round_2_security_vs_zero_cost(
        self,
        lens_evals: List[LensEvaluation],
        telemetry: TelemetryReport
    ) -> DebateRoundPayload:
        """Round 2: Security vs Zero-Cost (Pen Tester & Architect vs Auditor & CRO)."""
        pen_eval = next((e for e in lens_evals if e.lens_id == "the_pen_tester"), None)
        cro_eval = next((e for e in lens_evals if e.lens_id == "the_chief_risk_officer"), None)

        hard_veto_active = bool(pen_eval and pen_eval.hard_veto_triggered)
        veto_details = pen_eval.hard_veto_rationale if hard_veto_active else None

        budget_headroom = 100.0 if telemetry.monthly_budget_cap_usd == 0.0 else round(((telemetry.monthly_budget_cap_usd - telemetry.accumulated_spend_usd) / telemetry.monthly_budget_cap_usd) * 100, 1)

        grounding = TelemetryGrounding(
            reserve_headroom_observed_percent=round(100.0 - telemetry.observed_cpu_utilization_pct, 1),
            estimated_monthly_spend_usd=telemetry.accumulated_spend_usd,
            gemini_token_budget_headroom_percent=budget_headroom,
            is_within_budget_manifest=telemetry.budget_ok,
            is_within_50_percent_reserve=telemetry.is_within_50_percent_reserve
        )

        if hard_veto_active:
            status = "DEADLOCKED_VETO"
            concession_by = "Chamber 3: Risk, Legal & Armor"
            concession_terms = "P0 Security Hard Veto is absolute. Budget/reserve constraints yield unconditionally to patch P0 exploit without incurring paid costs."
            p0_issue = next((i for i in pen_eval.issues if i.severity == 5), None)
            approved_diff = p0_issue.proposed_diff if p0_issue else "// P0 Security patch mandated"
            issue_id = p0_issue.issue_id if p0_issue else "SEC-P0-VETO"
            sev = 5
        else:
            status = "CONSENSUS_REACHED"
            concession_by = "The Systems Architect"
            concession_terms = "Applied in-memory caching and connection pooling to keep resource usage within 50% reserve."
            approved_diff = "// Zero-cost cached security middleware"
            issue_id = "SEC-R2-001"
            sev = 2

        return DebateRoundPayload(
            round_id="round_2_security_vs_zero_cost",
            round_name="Round 2: Security vs. Zero-Cost Debate",
            chamber_a=ChamberParticipant(
                chamber_name="Chamber 2: Technical & Defense",
                advocate_lens="the_pen_tester",
                position="Mandate strict tenant boundary enforcement, JWT verification, and SQLi defense.",
                metric_claim="P0 Exploit Prevention."
            ),
            chamber_b=ChamberParticipant(
                chamber_name="Chamber 3: Risk, Legal & Armor",
                advocate_lens="the_chief_risk_officer",
                position="Enforce 50% system reserve and Strict Zero-Cost Invariant ($0.00 ceiling). No paid services allowed.",
                metric_claim="Spend: $0.00 / Paid Services: BLOCKED."
            ),
            hard_veto_asserted=hard_veto_active,
            veto_details=veto_details,
            trade_off_resolution=TradeOffResolution(
                issue_id=issue_id,
                severity=sev,
                concession_granted_by=concession_by,
                concession_terms=concession_terms,
                approved_diff=approved_diff
            ),
            telemetry_grounding=grounding,
            status=status
        )

    async def run_round_3_liability_vs_aggressive_copy(
        self,
        lens_evals: List[LensEvaluation]
    ) -> DebateRoundPayload:
        """Round 3: Liability vs Aggressive Copy (Wordsmith & Monetizer vs Legal Counsel)."""
        legal_eval = next((e for e in lens_evals if e.lens_id == "the_legal_counsel"), None)
        wordsmith_eval = next((e for e in lens_evals if e.lens_id == "the_wordsmith"), None)

        hard_veto_active = bool(legal_eval and legal_eval.hard_veto_triggered)
        veto_details = legal_eval.hard_veto_rationale if hard_veto_active else None

        if hard_veto_active:
            status = "DEADLOCKED_VETO"
            concession_by = "Chamber 1: Commercial & Growth"
            concession_terms = "Legal Counsel hard veto asserted. All deceptive claims or unconsented data collection revoked."
            p0_issue = next((i for i in legal_eval.issues if i.severity == 5), None)
            approved_diff = p0_issue.proposed_diff if p0_issue else "// Legal compliance patch mandated"
            issue_id = p0_issue.issue_id if p0_issue else "LEGAL-P0-VETO"
            sev = 5
        else:
            status = "COMPROMISE_ACCEPTED"
            concession_by = "The Wordsmith"
            concession_terms = "Updated candidate exam instructions and upload notices with compliant disclaimers without harming UX clarity."
            legal_issue = legal_eval.issues[0] if (legal_eval and legal_eval.issues) else None
            approved_diff = legal_issue.proposed_diff if legal_issue else "// Compliant disclaimer diff"
            issue_id = legal_issue.issue_id if legal_issue else "LEGAL-R3-001"
            sev = legal_issue.severity if legal_issue else 2

        return DebateRoundPayload(
            round_id="round_3_liability_vs_aggressive_copy",
            round_name="Round 3: Liability vs. Aggressive Copy",
            chamber_a=ChamberParticipant(
                chamber_name="Chamber 1: Commercial & Growth",
                advocate_lens="the_wordsmith",
                position="Maximize student exam completion and candidate onboarding with punchy, frictionless microcopy.",
                metric_claim="Higher assessment completion rates."
            ),
            chamber_b=ChamberParticipant(
                chamber_name="Chamber 3: Risk, Legal & Armor",
                advocate_lens="the_legal_counsel",
                position="Insulate platform from liability, copyright infringement on scanned papers, and student privacy violations.",
                metric_claim="FERPA / Regulatory compliance."
            ),
            hard_veto_asserted=hard_veto_active,
            veto_details=veto_details,
            trade_off_resolution=TradeOffResolution(
                issue_id=issue_id,
                severity=sev,
                concession_granted_by=concession_by,
                concession_terms=concession_terms,
                approved_diff=approved_diff
            ),
            status=status
        )

    async def run_phase_2_parallel_debates(
        self,
        lens_evals: List[LensEvaluation],
        telemetry: TelemetryReport
    ) -> List[DebateRoundPayload]:
        """Execute Round 1, Round 2, and Round 3 in parallel as isolated 1v1 context threads."""
        t1 = self.run_round_1_speed_vs_conversion(lens_evals)
        t2 = self.run_round_2_security_vs_zero_cost(lens_evals, telemetry)
        t3 = self.run_round_3_liability_vs_aggressive_copy(lens_evals)

        rounds = await asyncio.gather(t1, t2, t3)
        return list(rounds)

    # --------------------------------------------------------------------------
    # PHASE 3: Fan-in to Evolution Synthesizer
    # --------------------------------------------------------------------------
    def run_anti_obsolescence_check(
        self,
        diffs_to_check: List[str]
    ) -> AntiObsolescenceGate:
        """Scan diffs for deprecated patterns and attach binding upgrades."""
        gate_cfg = self.config.get("dag_execution", {}).get("anti_obsolescence_gate", {})
        disallowed = gate_cfg.get("disallowed_patterns", [".Result", ".Wait()", "Thread.Sleep", "md5"])

        detected = []
        upgrades = []

        combined_diff = "\n".join(diffs_to_check)
        for pattern in disallowed:
            if pattern in combined_diff:
                detected.append(pattern)
                if pattern in (".Result", ".Wait()"):
                    upgrades.append({
                        "package_or_pattern": pattern,
                        "upgrade_requirement": "Refactor to non-blocking 'await' with ConfigureAwait(false) in service layers."
                    })
                elif pattern.lower() in ("md5", "sha1"):
                    upgrades.append({
                        "package_or_pattern": pattern,
                        "upgrade_requirement": "Upgrade to SHA256 / HMACSHA256 for cryptographic signatures."
                    })
                elif "SELECT *" in pattern:
                    upgrades.append({
                        "package_or_pattern": "SELECT *",
                        "upgrade_requirement": "Specify explicit column projections in Dapper to avoid unneeded payload overhead."
                    })

        passed = len(detected) == 0
        return AntiObsolescenceGate(
            passed=passed,
            target_framework=gate_cfg.get("target_framework", "net8.0"),
            deprecated_apis_detected=detected,
            binding_upgrades=upgrades
        )

    async def run_phase_3_synthesizer(
        self,
        lens_evals: List[LensEvaluation],
        debate_rounds: List[DebateRoundPayload],
        target_file: Optional[str] = None
    ) -> ConsensusDecree:
        """Fan into the Evolution Synthesizer to emit the Consensus Decree."""
        # 1. Inspect Veto Registry
        pen_veto = any(e.hard_veto_triggered for e in lens_evals if e.lens_id == "the_pen_tester")
        legal_veto = any(e.hard_veto_triggered for e in lens_evals if e.lens_id == "the_legal_counsel")

        blocking_reasons = []
        for e in lens_evals:
            if e.hard_veto_triggered and e.hard_veto_rationale:
                blocking_reasons.append(f"[{e.lens_id.upper()}] {e.hard_veto_rationale}")

        veto_reg = VetoRegistry(
            pen_tester_veto_active=pen_veto,
            legal_counsel_veto_active=legal_veto,
            blocking_reasons=blocking_reasons
        )

        # 2. Extract approved diffs from debate resolutions & high severity lens issues
        diffs = []
        directives = []
        trade_offs = []

        for r in debate_rounds:
            res = r.trade_off_resolution
            diffs.append(res.approved_diff)
            trade_offs.append({
                "round": r.round_name,
                "topic": r.chamber_a.position + " vs " + r.chamber_b.position,
                "latency_impact": r.chamber_b.metric_claim or "N/A",
                "conversion_impact": r.chamber_a.metric_claim or "N/A",
                "accepted_rationale": res.concession_terms
            })

            # Formulate Staged Auto-Refactor Directive dynamically
            directive_id = f"DIR-{r.round_id.upper()}"
            curr_target = target_file or "ExamifyAPI/Controllers/ExamController.cs"
            comp_impact = "REQUIRES_DOTNET_BUILD" if curr_target.endswith(".cs") else "RUNTIME_INTERPRETED_NO_BUILD"
            verif_cmd = "dotnet test ExamiFy.Tests/ExamiFy.Tests.csproj" if curr_target.endswith(".cs") else "Verify view markup in browser"

            directives.append(AutoRefactorDirective(
                directive_id=directive_id,
                target_file=curr_target,
                action="MODIFY",
                summary=res.concession_terms,
                unified_diff=res.approved_diff,
                compilation_impact=comp_impact,
                verification_command=verif_cmd
            ))

        # 3. Anti-Obsolescence Gate
        obsolescence_gate = self.run_anti_obsolescence_check(diffs)

        # 4. Determine final verdict
        if pen_veto or legal_veto:
            verdict = "VETO_BLOCKED"
        elif not obsolescence_gate.passed:
            verdict = "CONSENSUS_DECREE_CONDITIONAL"
        else:
            verdict = "CONSENSUS_DECREE_APPROVED"

        decree_id = f"DECREE-{int(time.time())}"
        timestamp_utc = datetime.now(timezone.utc).isoformat()

        return ConsensusDecree(
            decree_id=decree_id,
            timestamp_utc=timestamp_utc,
            verdict=verdict,
            anti_obsolescence_gate=obsolescence_gate,
            veto_registry=veto_reg,
            weighted_trade_offs_summary=trade_offs,
            auto_refactor_directives=directives
        )

    # --------------------------------------------------------------------------
    # FULL DAG PIPELINE EXECUTION
    # --------------------------------------------------------------------------
    async def execute_dag(
        self,
        target_context: str,
        target_file: Optional[str] = None
    ) -> tuple[List[LensEvaluation], List[DebateRoundPayload], ConsensusDecree, TelemetryReport]:
        """Execute the complete end-to-end Parallelized Debate Graph."""
        print("[DAG] 1. Ingesting Dynamic Telemetry & Budget Manifest...")
        telemetry = self.telemetry_collector.collect()

        print("[DAG] 2. Phase 1 (Fan-out): Executing 10 Lens Evaluations concurrently...")
        lens_evals = await self.run_phase_1_fan_out(target_context)

        print("[DAG] 3. Phase 2: Running Round 1, Round 2, Round 3 in parallel as isolated 1v1 threads...")
        debate_rounds = await self.run_phase_2_parallel_debates(lens_evals, telemetry)

        print("[DAG] 4. Phase 3 (Fan-in): Synthesizing via The Evolution Synthesizer...")
        decree = await self.run_phase_3_synthesizer(lens_evals, debate_rounds, target_file=target_file)

        print(f"[DAG] Finished. Verdict: {decree.verdict}")
        return lens_evals, debate_rounds, decree, telemetry
